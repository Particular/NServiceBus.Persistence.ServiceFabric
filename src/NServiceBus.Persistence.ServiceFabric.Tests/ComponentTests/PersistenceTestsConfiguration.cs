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

        public Func<Action, ContextBag> Func;
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