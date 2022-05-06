namespace NServiceBus.Persistence.ServiceFabric
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Data;
    using Outbox;

    class ServiceFabricOutboxTransaction : IOutboxTransaction
    {
        internal StorageSession Session { get; }

        public ServiceFabricOutboxTransaction(IReliableStateManager stateManager, ITransaction transaction, TimeSpan transactionTimeout) =>
            Session = new StorageSession(stateManager, transaction, transactionTimeout, false);

        public void Dispose() => Session.Dispose();

        public Task Commit(CancellationToken cancellationToken = default) => Session.CompleteAsync(cancellationToken);
    }
}