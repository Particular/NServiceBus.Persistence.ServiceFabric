namespace NServiceBus.Persistence.ServiceFabric
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Data;
    using Outbox;

    class ServiceFabricOutboxTransaction : OutboxTransaction
    {
        internal IReliableStateManager StateManager { get; }
        internal Lazy<ITransaction> Transaction { get; }

        public ServiceFabricOutboxTransaction(IReliableStateManager stateManager)
        {
            StateManager = stateManager;
            Transaction = new Lazy<ITransaction>(stateManager.CreateTransaction);
        }

        public void Dispose()
        {
            if (Transaction.IsValueCreated)
            {
                Transaction.Value.Dispose();
            }
        }

        public Task Commit()
        {
            if (Transaction.IsValueCreated)
            {
                return Transaction.Value.CommitAsync();
            }

            return TaskEx.CompletedTask;
        }
    }
}