namespace NServiceBus.Persistence.ServiceFabric
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Data;
    using Outbox;

    class ServiceFabricOutboxTransaction : OutboxTransaction
    {
        internal ITransaction Transaction { get; }
        internal IReliableStateManager StateManager { get; }
        internal TimeSpan TransactionTimeout { get; }

        public ServiceFabricOutboxTransaction(IReliableStateManager stateManager, ITransaction transaction, TimeSpan transactionTimeout)
        {
            StateManager = stateManager;
            Transaction = transaction;
            TransactionTimeout = transactionTimeout;
        }

        public void Dispose()
        {
            Transaction.Dispose();
        }

        public Task Commit()
        {
            return Transaction.CommitAsync();
        }
    }
}