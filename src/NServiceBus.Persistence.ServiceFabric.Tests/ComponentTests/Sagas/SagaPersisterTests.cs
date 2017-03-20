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

        protected SagaCorrelationProperty SetActiveSagaInstance<TSaga, TSagaData>(ContextBag context, TSaga saga, TSagaData sagaData, params Type[] availableTypes)
            where TSaga : Saga<TSagaData>
            where TSagaData : IContainSagaData, new()
        {
            var sagaMetadata = SagaMetadata.Create(typeof(TSaga), availableTypes, new Conventions());
            var sagaInstance = new ActiveSagaInstance(saga, sagaMetadata, () => DateTime.UtcNow);
            var correlationProperty = SagaCorrelationProperty.None;
            SagaMetadata.CorrelationPropertyMetadata correlatedProp;
            if (sagaMetadata.TryGetCorrelationProperty(out correlatedProp))
            {
                var prop = sagaData.GetType().GetProperty(correlatedProp.Name);

                var value = prop.GetValue(sagaData);

                correlationProperty = new SagaCorrelationProperty(correlatedProp.Name, value);
            }

            if (sagaData.Id == Guid.Empty)
            {
                sagaData.Id = configuration.SagaIdGenerator.Generate(new SagaIdGeneratorContext(correlationProperty, sagaMetadata, context));
            }
            sagaInstance.AttachNewEntity(sagaData);
            context.Set(sagaInstance);

            return correlationProperty;
        }
    }
}