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
        IReliableStateManager stateManager;

        public PersistenceTestsConfiguration()
        {
            var statefulService = (StatefulService) TestContext.CurrentContext.Test.Properties.Get("StatefulService");
            stateManager = statefulService.StateManager;

            SynchronizedStorage = new SynchronizedStorage(stateManager);
            SagaStorage = new ServiceFabricSagaPersister();
            OutboxStorage = new ServiceFabricOutboxStorage();

            var timeouts = new ServiceFabricTimeoutPersister(()=>DateTime.Now);
            TimeoutStorage = timeouts;
            TimeoutQuery = timeouts;

            SubscriptionStorage = new ServiceFabricSubscriptionStorage();
        }

        public ISagaPersister SagaStorage { get; }
        public ISynchronizedStorage SynchronizedStorage { get; }
        public ISubscriptionStorage SubscriptionStorage { get; }
        public IPersistTimeouts TimeoutStorage { get; }
        public IQueryTimeouts TimeoutQuery { get; }
        public IOutboxStorage OutboxStorage { get; }
        public IDeduplicateMessages GatewayStorage { get; }

        public async Task Configure()
        {
            await stateManager.RegisterDictionaries().ConfigureAwait(false);
            using (var transaction = stateManager.CreateTransaction())
            {
                var sagas = await stateManager.Sagas(transaction).ConfigureAwait(false);
                var correlations = await stateManager.Correlations(transaction).ConfigureAwait(false);
                var junkKey = Guid.NewGuid();// needs to be a const
                await sagas.GetOrAddAsync(transaction, junkKey, guid => new SagaEntry()).ConfigureAwait(false);
                var junkCorrelationPropertyKey = new CorrelationPropertyEntry();
                await correlations.GetOrAddAsync(transaction, junkCorrelationPropertyKey, key => junkKey).ConfigureAwait(false);
                await transaction.CommitAsync().ConfigureAwait(false);
            }
        }

        public Task Cleanup()
        {
            return Task.FromResult(0);
        }
    }
}