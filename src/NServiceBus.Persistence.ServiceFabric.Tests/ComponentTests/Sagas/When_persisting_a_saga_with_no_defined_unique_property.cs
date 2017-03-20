namespace NServiceBus.Persistence.ComponentTests
{
    using System;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class When_persisting_a_saga_with_no_defined_unique_property : SagaPersisterTests
    {
        [Test]
        public async Task It_should_persist_successfully()
        {
            configuration.RequiresFindersSupport();

            var propertyData = Guid.NewGuid().ToString();
            var sagaData = new SagaWithoutCorrelationPropertyData { FoundByFinderProperty = propertyData };

            var persister = configuration.SagaStorage;
            var savingContextBag = configuration.GetContextBagForSagaStorage();
            Guid generateSagaId;
            using (var session = await configuration.SynchronizedStorage.OpenSession(savingContextBag))
            {
                var correlationPropertyNone = SetActiveSagaInstance(savingContextBag, new SagaWithoutCorrelationProperty(), sagaData, typeof(CustomFinder));
                generateSagaId = sagaData.Id;

                await persister.Save(sagaData, correlationPropertyNone, session, savingContextBag);
                await session.CompleteAsync();
            }

            var readContextBag = configuration.GetContextBagForSagaStorage();
            using (var readSession = await configuration.SynchronizedStorage.OpenSession(readContextBag))
            {
                SetActiveSagaInstance(readContextBag, new SagaWithoutCorrelationProperty(), new SagaWithoutCorrelationPropertyData { FoundByFinderProperty = propertyData }, typeof(CustomFinder));

                var result = await persister.Get<SagaWithoutCorrelationPropertyData>(generateSagaId, readSession, readContextBag);
                Assert.AreEqual(sagaData.FoundByFinderProperty, result.FoundByFinderProperty);
            }
        }
    }
}