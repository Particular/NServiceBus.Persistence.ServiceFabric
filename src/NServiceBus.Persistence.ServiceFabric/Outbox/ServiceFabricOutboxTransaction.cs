namespace NServiceBus.Persistence.ServiceFabric.Outbox
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Data;
    using NServiceBus.Outbox;

   // [SkipWeaving]
    class ServiceFabricOutboxTransaction : OutboxTransaction
    {
        // ReSharper disable once EmptyConstructor
        public ServiceFabricOutboxTransaction(IReliableStateManager stateManager)
        {
            StateManager = stateManager;
            Transaction = stateManager.CreateTransaction();
        }

        internal IReliableStateManager StateManager { get; set; }
        internal ITransaction Transaction { get; set; }
        
        public void Dispose()
        {
           Transaction.Dispose();
        }

        public Task Commit()
        {
            return Transaction.CommitAsync();
        }

        public Task Abort()
        {
            Transaction.Abort();

            return TaskEx.CompletedTask;
        }

    }
}