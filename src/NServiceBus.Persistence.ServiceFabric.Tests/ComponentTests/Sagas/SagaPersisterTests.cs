namespace NServiceBus.Persistence.ComponentTests
{
    using System;
    using System.Threading.Tasks;
    using Extensibility;
    using NUnit.Framework;
    using Sagas;

    public class SagaPersisterTests
    {
        protected PersistenceTestsConfiguration configuration;

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            configuration = new PersistenceTestsConfiguration();
            await configuration.Configure();
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            await configuration.Cleanup();
        }

        protected static SagaCorrelationProperty SetActiveSagaInstance<TSaga, TSagaData>(ContextBag savingContextBag, TSaga saga, TSagaData sagaData)
            where TSaga : Saga<TSagaData>
            where TSagaData : IContainSagaData, new()
        {
            var sagaInstance = new ActiveSagaInstance(saga, SagaMetadata.Create(typeof(TSaga)), () => DateTime.UtcNow);
            sagaInstance.AttachNewEntity(sagaData);
            savingContextBag.Set(sagaInstance);
            return SagaMetadataHelper.GetMetadata<TSaga>(sagaData);
        }
    }
}