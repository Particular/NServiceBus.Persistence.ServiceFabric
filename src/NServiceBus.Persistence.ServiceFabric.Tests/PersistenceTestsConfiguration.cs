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
            SagaStorage = new SagaPersister();
            OutboxStorage = new OutboxStorage(statefulService.StateManager);
        }

        IReliableStateManager stateManager;

        public bool SupportsDtc { get; } = false;
        public bool SupportsOutbox { get; } = true;
        public bool SupportsFinders { get; } = true;
        public bool SupportsSubscriptions { get; } = false;
        public bool SupportsTimeouts { get; } = false;

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
            await stateManager.RegisterSagaStorage((SagaPersister)SagaStorage).ConfigureAwait(false);
            await stateManager.RegisterOutboxStorage((OutboxStorage)OutboxStorage).ConfigureAwait(false);
        }

        public Task Cleanup()
        {
            return Task.FromResult(0);
        }

        public Task CleanupMessagesOlderThan(DateTimeOffset beforeStore)
        {
            return ((OutboxStorage) OutboxStorage).CleanupMessagesOlderThan(beforeStore, CancellationToken.None);
        }
    }
}