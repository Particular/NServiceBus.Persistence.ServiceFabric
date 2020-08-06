// ReSharper disable UnusedMemberInSuper.Global
namespace NServiceBus.Persistence.ComponentTests
{
    using System;
    using System.Threading.Tasks;
    using Outbox;
    using Timeout.Core;
    using Unicast.Subscriptions.MessageDrivenSubscriptions;

    public interface IPersistenceTestsConfiguration
    {
        bool SupportsDtc { get; }

        bool SupportsOutbox { get; }

        bool SupportsFinders { get; }

        bool SupportsSubscriptions { get; }

        bool SupportsTimeouts { get; }

        bool SupportsOptimisticConcurrency { get; }

        bool SupportsPessimisticConcurrency { get; }

        ISynchronizedStorage SynchronizedStorage { get; }

        ISynchronizedStorageAdapter SynchronizedStorageAdapter { get; }

        ISubscriptionStorage SubscriptionStorage { get; }

        IPersistTimeouts TimeoutStorage { get; }

        IQueryTimeouts TimeoutQuery { get; }

        IOutboxStorage OutboxStorage { get; }

        Task Configure();

        Task Cleanup();

        Task CleanupMessagesOlderThan(DateTimeOffset beforeStore);
    }
}