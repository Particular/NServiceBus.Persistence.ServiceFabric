namespace NServiceBus.Persistence.ServiceFabric
{
    using System.Threading.Tasks;
    using Outbox;

    class ServiceFabricOutboxTransaction : OutboxTransaction
    {
        internal ServiceFabricTransaction Transaction { get; }

        public ServiceFabricOutboxTransaction(ServiceFabricTransaction transaction)
        {
            Transaction = transaction;
        }

        public void Dispose()
        {
            Transaction.Dispose();
        }

        public Task Commit()
        {
            return Transaction.Commit();
        }
    }
}