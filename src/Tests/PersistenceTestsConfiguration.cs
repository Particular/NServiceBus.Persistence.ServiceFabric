﻿namespace NServiceBus.Persistence.ComponentTests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Data;
    using NUnit.Framework;
    using Outbox;
    using Sagas;
    using ServiceFabric;
    using Unicast.Subscriptions.MessageDrivenSubscriptions;
    using StatefulService = Microsoft.ServiceFabric.Services.Runtime.StatefulService;

    public partial class PersistenceTestsConfiguration
    {
        public PersistenceTestsConfiguration(TimeSpan? transactionTimeout = null)
        {
            var statefulService = (StatefulService)TestContext.CurrentContext.Test.Properties.Get("StatefulService");
            stateManager = statefulService.StateManager;

            var timeout = transactionTimeout ?? TimeSpan.FromSeconds(4);
            SynchronizedStorage = new SynchronizedStorage(stateManager, timeout);
            SynchronizedStorageAdapter = new SynchronizedStorageAdapter();

            OutboxStorage = new OutboxStorage(statefulService.StateManager, timeout);
        }

        IReliableStateManager stateManager;

        public bool SupportsDtc { get; } = false;
        public bool SupportsOutbox { get; } = true;
        public bool SupportsFinders { get; } = false;
        public bool SupportsSubscriptions { get; } = false;
        public bool SupportsTimeouts { get; } = false;
        public bool SupportsOptimisticConcurrency { get; } = false;
        public bool SupportsPessimisticConcurrency { get; } = true;
        public ISagaIdGenerator SagaIdGenerator { get; }

        public ISynchronizedStorage SynchronizedStorage { get; }
        public ISynchronizedStorageAdapter SynchronizedStorageAdapter { get; }
        public ISubscriptionStorage SubscriptionStorage { get; }
        public IOutboxStorage OutboxStorage { get; }

        public async Task Configure(CancellationToken cancellationToken = default)
        {
            await stateManager.RegisterOutboxStorage((OutboxStorage)OutboxStorage, cancellationToken).ConfigureAwait(false);
        }

        public Task Cleanup(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0);
        }

        public Task CleanupMessagesOlderThan(DateTimeOffset beforeStore, CancellationToken cancellationToken = default)
        {
            var storage = (OutboxStorage)OutboxStorage;
            return storage.CleanUpOutboxQueue(beforeStore, cancellationToken);
        }
    }
}