namespace NServiceBus.PersistenceTesting
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Services.Runtime;
    using NServiceBus.Outbox;
    using NServiceBus.Sagas;
    using NUnit.Framework;
    using Persistence;
    using Persistence.ServiceFabric;

    public partial class PersistenceTestsConfiguration
    {
        public bool SupportsDtc => false;
        public bool SupportsOutbox => false;
        public bool SupportsFinders => false;
        public bool SupportsSubscriptions => true;
        public bool SupportsTimeouts => true;
        public bool SupportsPessimisticConcurrency => false;

        public ISagaIdGenerator SagaIdGenerator { get; set; }
        public ISagaPersister SagaStorage { get; set; }
        public ISynchronizedStorage SynchronizedStorage { get; set; }
        public ISynchronizedStorageAdapter SynchronizedStorageAdapter { get; set; }
        public IOutboxStorage OutboxStorage { get; }

        public PersistenceTestsConfiguration()
        {
            var statefulService = (StatefulService)TestContext.CurrentContext.Test.Properties.Get("StatefulService");
            stateManager = statefulService.StateManager;

            var timeout = TimeSpan.FromSeconds(4);
            SynchronizedStorage = new SynchronizedStorage(stateManager, timeout);
            SynchronizedStorageAdapter = new SynchronizedStorageAdapter();

            var sagaInfoCache = new SagaInfoCache();
            sagaInfoCache.Initialize(SagaMetadataCollection);
            SagaStorage = new SagaPersister(sagaInfoCache);

            var sagaIdGenerator = new SagaIdGenerator();
            sagaIdGenerator.Initialize(sagaInfoCache);
            SagaIdGenerator = sagaIdGenerator;

            OutboxStorage = new OutboxStorage(statefulService.StateManager, timeout);
        }

        public async Task Configure()
        {
            await stateManager.RegisterOutboxStorage((OutboxStorage)OutboxStorage).ConfigureAwait(false);
        }

        public Task Cleanup()
        {
            return Task.CompletedTask;
        }

        IReliableStateManager stateManager;
    }
}