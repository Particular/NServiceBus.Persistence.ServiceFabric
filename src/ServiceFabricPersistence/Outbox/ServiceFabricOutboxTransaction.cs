namespace NServiceBus.Persistence.ServiceFabric
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Data;
    using Outbox;

    class ServiceFabricOutboxTransaction : IOutboxTransaction
    {
        public ServiceFabricOutboxTransaction(IReliableStateManager stateManager) =>
            Transaction = stateManager.CreateTransaction();

        internal ITransaction Transaction { get; }

        public void Dispose() => Transaction.Dispose();

        public Task Commit(CancellationToken cancellationToken = default) => Transaction.CommitAsync();
    }
}