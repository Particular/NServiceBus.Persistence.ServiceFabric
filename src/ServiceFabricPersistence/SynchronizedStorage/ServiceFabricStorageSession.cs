namespace NServiceBus.Persistence.ServiceFabric
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Extensibility;
    using Microsoft.ServiceFabric.Data;
    using Outbox;
    using Transport;

    class ServiceFabricStorageSession : ICompletableSynchronizedStorageSession, IServiceFabricStorageSession
    {
        readonly IReliableStateManager stateManager;
        readonly TimeSpan transactionTimeout;

        public ServiceFabricStorageSession(IReliableStateManager stateManager, TimeSpan transactionTimeout)
        {
            this.transactionTimeout = transactionTimeout;
            this.stateManager = stateManager;
        }

        public void Dispose() => Session.Dispose();

        public ValueTask<bool> TryOpen(IOutboxTransaction transaction, ContextBag context,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (transaction is ServiceFabricOutboxTransaction outboxTransaction)
            {
                Session = outboxTransaction.Session;
                return new ValueTask<bool>(true);
            }
            return new ValueTask<bool>(false);
        }

        public ValueTask<bool> TryOpen(TransportTransaction transportTransaction, ContextBag context,
            CancellationToken cancellationToken = new CancellationToken()) =>
            new ValueTask<bool>(false);

        public Task Open(ContextBag contextBag, CancellationToken cancellationToken = new CancellationToken())
        {
            Session = new StorageSession(stateManager, stateManager.CreateTransaction(), transactionTimeout, ownsTransaction: true);
            return Task.CompletedTask;
        }

        public Task CompleteAsync(CancellationToken cancellationToken = default) => Session.CompleteAsync(cancellationToken);

        public IReliableStateManager StateManager => Session.StateManager;

        public ITransaction Transaction => Session.Transaction;

        public TimeSpan TransactionTimeout => Session.TransactionTimeout;

        internal StorageSession Session { get; private set; }
    }
}