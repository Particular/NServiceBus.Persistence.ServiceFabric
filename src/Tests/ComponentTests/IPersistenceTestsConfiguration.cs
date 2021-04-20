namespace NServiceBus.Persistence.ComponentTests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Outbox;
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

        IOutboxStorage OutboxStorage { get; }

        Task Configure(CancellationToken cancellationToken = default);

        Task Cleanup(CancellationToken cancellationToken = default);

        Task CleanupMessagesOlderThan(DateTimeOffset beforeStore, CancellationToken cancellationToken = default);
    }
}