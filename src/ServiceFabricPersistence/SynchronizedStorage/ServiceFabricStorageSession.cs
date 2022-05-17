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
        public ServiceFabricStorageSession(IReliableStateManager stateManager, TimeSpan transactionTimeout)
        {
            TransactionTimeout = transactionTimeout;
            StateManager = stateManager;
        }

        public IReliableStateManager StateManager { get; }

        public ITransaction Transaction { get; private set; }

        public TimeSpan TransactionTimeout { get; }

        public void Dispose()
        {
            if (!disposed && ownsTransaction)
            {
                Transaction.Dispose();
                disposed = true;
            }
        }

        public ValueTask<bool> TryOpen(IOutboxTransaction transaction, ContextBag context,
            CancellationToken cancellationToken = default)
        {
            if (transaction is ServiceFabricOutboxTransaction outboxTransaction)
            {
                Transaction = outboxTransaction.Transaction;
                ownsTransaction = false;
                return new ValueTask<bool>(true);
            }

            return new ValueTask<bool>(false);
        }

        public ValueTask<bool> TryOpen(TransportTransaction transportTransaction, ContextBag context,
            CancellationToken cancellationToken = default) =>
            new ValueTask<bool>(false);

        public Task Open(ContextBag contextBag, CancellationToken cancellationToken = default)
        {
            ownsTransaction = true;
            Transaction = StateManager.CreateTransaction();
            return Task.CompletedTask;
        }

        public Task CompleteAsync(CancellationToken cancellationToken = default) =>
            ownsTransaction ? Transaction.CommitAsync() : Task.CompletedTask;

        bool ownsTransaction;
        bool disposed;
    }
}