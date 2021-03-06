namespace NServiceBus.Persistence.ServiceFabric
{
    using System.Threading;
    using System.Threading.Tasks;
    using Extensibility;
    using Outbox;
    using Transport;

    class SynchronizedStorageAdapter : ISynchronizedStorageAdapter
    {
        public Task<CompletableSynchronizedStorageSession> TryAdapt(OutboxTransaction transaction, ContextBag context, CancellationToken cancellationToken = default)
        {
            if (transaction is ServiceFabricOutboxTransaction outboxTransaction)
            {
                // this session will not own the transaction, the outbox part owns the transaction
                CompletableSynchronizedStorageSession session = new StorageSession(outboxTransaction.StateManager, outboxTransaction.Transaction, outboxTransaction.TransactionTimeout, false);
                return Task.FromResult(session);
            }
            return EmptyTask;
        }

        public Task<CompletableSynchronizedStorageSession> TryAdapt(TransportTransaction transportTransaction, ContextBag context, CancellationToken cancellationToken = default)
        {
            return EmptyTask;
        }

        static readonly Task<CompletableSynchronizedStorageSession> EmptyTask = Task.FromResult<CompletableSynchronizedStorageSession>(null);
    }
}