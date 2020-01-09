namespace NServiceBus.Persistence.ServiceFabric
{
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Data;
    using Outbox;

    class ServiceFabricOutboxTransaction : OutboxTransaction
    {
        internal ITransaction Transaction { get; }
        internal IReliableStateManager StateManager { get; }

        public ServiceFabricOutboxTransaction(IReliableStateManager stateManager, ITransaction transaction)
        {
            StateManager = stateManager;
            Transaction = transaction;
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