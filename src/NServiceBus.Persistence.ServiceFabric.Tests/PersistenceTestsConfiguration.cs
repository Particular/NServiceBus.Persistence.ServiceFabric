namespace NServiceBus.Persistence.ComponentTests
{
    using System.Threading.Tasks;
    using Gateway.Deduplication;
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

            SynchronizedStorage = new SynchronizedStorage(statefulService.StateManager);
        }

        public ISagaPersister SagaStorage { get; }
        public ISynchronizedStorage SynchronizedStorage { get; }
        public ISubscriptionStorage SubscriptionStorage { get; }
        public IPersistTimeouts TimeoutStorage { get; }
        public IQueryTimeouts TimeoutQuery { get; }
        public IOutboxStorage OutboxStorage { get; }
        public IDeduplicateMessages GatewayStorage { get; }

        public Task Configure()
        {
            return Task.FromResult(0);
        }

        public Task Cleanup()
        {
            return Task.FromResult(0);
        }
    }
}