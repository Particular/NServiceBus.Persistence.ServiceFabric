namespace NServiceBus.PersistenceTesting
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Data;
    using NServiceBus.Outbox;
    using NServiceBus.Sagas;
    using NUnit.Framework;
    using Persistence;
    using Persistence.ServiceFabric;
    using StatefulService = Microsoft.ServiceFabric.Services.Runtime.StatefulService;

    public partial class PersistenceTestsConfiguration
    {
        public bool SupportsDtc { get; } = false;

        public bool SupportsOutbox { get; } = true;

        public bool SupportsFinders { get; } = false;

        public bool SupportsPessimisticConcurrency { get; } = true;

        public ISagaIdGenerator SagaIdGenerator { get; set; }

        public ISagaPersister SagaStorage { get; set; }

        public ISynchronizedStorage SynchronizedStorage { get; set; }

        public ISynchronizedStorageAdapter SynchronizedStorageAdapter { get; set; }

        public IOutboxStorage OutboxStorage { get; set; }

        public async Task Configure(CancellationToken cancellationToken = default)
        {
            var statefulService = (StatefulService)TestContext.CurrentContext.Test.Properties.Get("StatefulService");
            stateManager = statefulService.StateManager;

            var timeout = SessionTimeout ?? TimeSpan.FromSeconds(4);
            SynchronizedStorage = new SynchronizedStorage(stateManager, timeout);
            SynchronizedStorageAdapter = new SynchronizedStorageAdapter();

            var sagaInfoCache = new SagaInfoCache();
            sagaInfoCache.Initialize(SagaMetadataCollection);
            SagaStorage = new SagaPersister(sagaInfoCache);

            var sagaIdGenerator = new SagaIdGenerator();
            sagaIdGenerator.Initialize(sagaInfoCache);
            SagaIdGenerator = sagaIdGenerator;

            OutboxStorage = new OutboxStorage(statefulService.StateManager, timeout);
            await stateManager.RegisterOutboxStorage((OutboxStorage)OutboxStorage).ConfigureAwait(false);
        }

        public Task Cleanup(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        IReliableStateManager stateManager;
    }
}