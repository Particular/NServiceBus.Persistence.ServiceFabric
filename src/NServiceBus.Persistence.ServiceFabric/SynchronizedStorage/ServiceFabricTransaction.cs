namespace NServiceBus.Persistence.ServiceFabric
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Data;

    class ServiceFabricTransaction : IDisposable
    {
        public ServiceFabricTransaction(IReliableStateManager stateManager)
        {
            StateManager = stateManager;
            actions = new List<Func<ITransaction, Task>>();
            transaction = new Lazy<ITransaction>(StateManager.CreateTransaction);
        }

        public IReliableStateManager StateManager { get; }

        public ITransaction Transaction => transaction.Value;

        public void Dispose()
        {
            actions.Clear();

            if (transaction.IsValueCreated)
            {
                Transaction.Dispose();
            }
        }

        public Task Add(Func<ITransaction, Task> action)
        {
            actions.Add(action);
            return TaskEx.CompletedTask;
        }

        public async Task Commit()
        {
            foreach (var action in actions)
            {
                await action(Transaction).ConfigureAwait(false);
            }

            if (transaction.IsValueCreated)
            {
                await Transaction.CommitAsync().ConfigureAwait(false);
            }
        }

        // this will lead to closure allocations
        List<Func<ITransaction, Task>> actions;

        Lazy<ITransaction> transaction;
    }
}