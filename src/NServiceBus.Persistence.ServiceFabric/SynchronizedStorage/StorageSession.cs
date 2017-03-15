namespace NServiceBus.Persistence.ServiceFabric
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Data;

    class StorageSession : CompletableSynchronizedStorageSession, IServiceFabricStorageSession
    {
        Lazy<ITransaction> _transaction;

        public StorageSession(IReliableStateManager stateManager, Lazy<ITransaction> transaction)
        {
            StateManager = stateManager;
            _transaction = transaction;
            actions = new List<Func<ITransaction, Task>>();
        }

        public IReliableStateManager StateManager { get; }

        public ITransaction Transaction => _transaction.Value;

        // this will lead to closure allocations
        List<Func<ITransaction, Task>> actions;

        public void Dispose()
        {
            actions.Clear();

            if (_transaction.IsValueCreated)
            {
                _transaction.Value.Dispose();
            }
        }

        public Task Add(Func<ITransaction, Task> action)
        {
            actions.Add(action);
            return TaskEx.CompletedTask;
        }

        public async Task CompleteAsync()
        {
            foreach (var action in actions)
            {
                await action(Transaction).ConfigureAwait(false);
            }

            if (_transaction.IsValueCreated)
            {
                await _transaction.Value.CommitAsync().ConfigureAwait(false);
            }
        }
    }
}