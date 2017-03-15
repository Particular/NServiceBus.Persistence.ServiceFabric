namespace NServiceBus.Persistence.ComponentTests
{
    using System;
    using System.Threading.Tasks;
    using Gateway.Deduplication;
    using Microsoft.ServiceFabric.Data;
    using NUnit.Framework;
    using Outbox;
    using Sagas;
    using ServiceFabric;
    using ServiceFabric.Outbox;
    using ServiceFabric.SagaPersister;
    using ServiceFabric.SubscriptionStorage;
    using ServiceFabric.TimeoutPersister;
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
            OutboxStorage = new ServiceFabricOutboxStorage(statefulService.StateManager);

            var timeouts = new ServiceFabricTimeoutPersister(()=>DateTime.UtcNow);
            TimeoutStorage = timeouts;
            TimeoutQuery = timeouts;

            SubscriptionStorage = new ServiceFabricSubscriptionStorage();
        }

        IReliableStateManager stateManager;

        public bool SupportsDtc { get; } = false;
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
            await stateManager.RegisterOutboxStorage().ConfigureAwait(false);
        }

        public Task Cleanup()
        {
            return Task.FromResult(0);
        }
    }
}