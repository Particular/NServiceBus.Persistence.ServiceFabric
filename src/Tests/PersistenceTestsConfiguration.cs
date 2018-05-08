namespace NServiceBus.Persistence.ComponentTests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Gateway.Deduplication;
    using Microsoft.ServiceFabric.Data;
    using NUnit.Framework;
    using Outbox;
    using Sagas;
    using ServiceFabric;
    using Timeout.Core;
    using Unicast.Subscriptions.MessageDrivenSubscriptions;
    using StatefulService = Microsoft.ServiceFabric.Services.Runtime.StatefulService;

    public partial class PersistenceTestsConfiguration
    {
        public PersistenceTestsConfiguration()
        {
            var statefulService = (StatefulService) TestContext.CurrentContext.Test.Properties.Get("StatefulService");
            stateManager = statefulService.StateManager;

            SynchronizedStorage = new SynchronizedStorage(stateManager);
            SynchronizedStorageAdapter = new SynchronizedStorageAdapter();

            var sagaInfoCache = new SagaInfoCache();
            sagaInfoCache.Initialize(SagaMetadataCollection);
            SagaStorage = new SagaPersister(sagaInfoCache);

            var sagaIdGenerator = new SagaIdGenerator();
            sagaIdGenerator.Initialize(sagaInfoCache);
            SagaIdGenerator = sagaIdGenerator;

            OutboxStorage = new OutboxStorage(statefulService.StateManager);
        }

        IReliableStateManager stateManager;

        public bool SupportsDtc { get; } = false;
        public bool SupportsOutbox { get; } = true;
        public bool SupportsFinders { get; } = true;
        public bool SupportsSubscriptions { get; } = false;
        public bool SupportsTimeouts { get; } = false;
        public ISagaIdGenerator SagaIdGenerator { get; }

        public ISagaPersister SagaStorage { get; }
        public ISynchronizedStorage SynchronizedStorage { get; }
        public ISynchronizedStorageAdapter SynchronizedStorageAdapter { get; }
        public ISubscriptionStorage SubscriptionStorage { get; }
        public IPersistTimeouts TimeoutStorage { get; }
        public IQueryTimeouts TimeoutQuery { get; }
        public IOutboxStorage OutboxStorage { get; }
        public IDeduplicateMessages GatewayStorage { get; }

        public async Task Configure()
        {
            await stateManager.RegisterOutboxStorage((OutboxStorage)OutboxStorage).ConfigureAwait(false);
        }

        public Task Cleanup()
        {
            return Task.FromResult(0);
        }

        public async Task CleanupMessagesOlderThan(DateTimeOffset beforeStore)
        {
            var storage = (OutboxStorage) OutboxStorage;
            await storage.CleanUpOldOutboxQueue(beforeStore, CancellationToken.None);
            await storage.CleanUpNewOutboxQueue(beforeStore, CancellationToken.None);
        }
    }
}