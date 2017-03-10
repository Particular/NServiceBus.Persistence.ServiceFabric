namespace NServiceBus.Persistence.ComponentTests
{
    using System;
    using System.Threading.Tasks;
    using Extensibility;
    using Gateway.Deduplication;
    using Outbox;
    using Sagas;
    using Timeout.Core;
    using Unicast.Subscriptions.MessageDrivenSubscriptions;

    public partial class PersistenceTestsConfiguration : IPersistenceTestsConfiguration
    {
        public Func<ContextBag> GetContextBagForTimeoutPersister { get; set; } = () => new ContextBag();
        public Func<ContextBag> GetContextBagForSagaStorage { get; set; } = () => new ContextBag();
        public Func<ContextBag> GetContextBagForOutbox { get; set; } = () => new ContextBag();
        public Func<ContextBag> GetContextBagForSubscriptions { get; set; } = () => new ContextBag();
    }

    public interface IPersistenceTestsConfiguration
    {
        ISagaPersister SagaStorage { get; }

        ISynchronizedStorage SynchronizedStorage { get; }

        ISubscriptionStorage SubscriptionStorage { get; }

        IPersistTimeouts TimeoutStorage { get; }

        IQueryTimeouts TimeoutQuery { get; }

        IOutboxStorage OutboxStorage { get; }

        IDeduplicateMessages GatewayStorage { get; }

        Task Configure();

        Task Cleanup();
    }
}