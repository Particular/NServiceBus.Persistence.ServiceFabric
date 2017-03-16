namespace NServiceBus.Persistence.ServiceFabric
{
    using System;
    using System.Threading.Tasks;
    using Extensibility;
    using Microsoft.ServiceFabric.Data;
    using NServiceBus.Outbox;
   using Transport;

    class SynchronizedStorageAdapter : ISynchronizedStorageAdapter
    {
        public Task<CompletableSynchronizedStorageSession> TryAdapt(OutboxTransaction transaction, ContextBag context)
        {
            var outboxTransaction = transaction as ServiceFabricOutboxTransaction;
            if (outboxTransaction != null)
            {
                CompletableSynchronizedStorageSession session = new StorageSession(outboxTransaction.StateManager, new Lazy<ITransaction>(() => outboxTransaction.Transaction));
                return Task.FromResult(session);
            }
            return EmptyTask;
        }

        public Task<CompletableSynchronizedStorageSession> TryAdapt(TransportTransaction transportTransaction, ContextBag context)
        {
            return EmptyTask;
        }

        static readonly Task<CompletableSynchronizedStorageSession> EmptyTask = Task.FromResult<CompletableSynchronizedStorageSession>(null);
    }
}