namespace NServiceBus.Persistence.ServiceFabric
{
    using System.Threading;
    using System.Threading.Tasks;
    using Extensibility;
    using Outbox;
    using Transport;

    class SynchronizedStorageAdapter : ISynchronizedStorageAdapter
    {
        public Task<ICompletableSynchronizedStorageSession> TryAdapt(IOutboxTransaction transaction, ContextBag context, CancellationToken cancellationToken = default)
        {
            if (transaction is ServiceFabricOutboxTransaction outboxTransaction)
            {
                // this session will not own the transaction, the outbox part owns the transaction
                ICompletableSynchronizedStorageSession session = new StorageSession(outboxTransaction.StateManager, outboxTransaction.Transaction, outboxTransaction.TransactionTimeout, false);
                return Task.FromResult(session);
            }
            return EmptyTask;
        }

        public Task<ICompletableSynchronizedStorageSession> TryAdapt(TransportTransaction transportTransaction, ContextBag context, CancellationToken cancellationToken = default)
        {
            return EmptyTask;
        }

        static readonly Task<ICompletableSynchronizedStorageSession> EmptyTask = Task.FromResult<ICompletableSynchronizedStorageSession>(null);
    }
}