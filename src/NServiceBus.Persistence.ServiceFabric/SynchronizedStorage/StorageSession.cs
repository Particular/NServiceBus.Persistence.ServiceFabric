namespace NServiceBus.Persistence.ServiceFabric
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Data;

    class StorageSession : CompletableSynchronizedStorageSession, IServiceFabricStorageSession
    {
        Lazy<ITransaction> transaction;

        public StorageSession(IReliableStateManager stateManager, Lazy<ITransaction> transaction)
        {
            StateManager = stateManager;
            this.transaction = transaction;
            actions = new List<Func<ITransaction, Task>>();
        }

        public IReliableStateManager StateManager { get; }

        public ITransaction Transaction => transaction.Value;

        // this will lead to closure allocations
        List<Func<ITransaction, Task>> actions;

        public void Dispose()
        {
            actions.Clear();

            if (transaction.IsValueCreated)
            {
                transaction.Value.Dispose();
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

            if (transaction.IsValueCreated)
            {
                await transaction.Value.CommitAsync().ConfigureAwait(false);
            }
        }
    }
}