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
            var sagaData = new SagaWithoutCorrelationPropertyData
            {
                Id = Guid.NewGuid(),
                CorrelatedProperty = "whatever"
            };

            var persister = configuration.SagaStorage;
            var savingContextBag = configuration.GetContextBagForSagaStorage();
            using (var session = await configuration.SynchronizedStorage.OpenSession(savingContextBag))
            {
                SetActiveSagaInstance(savingContextBag, new SagaWithoutCorrelationProperty(), sagaData);

                await persister.Save(sagaData, null, session, savingContextBag);
                await session.CompleteAsync();
            }

            var readContextBag = configuration.GetContextBagForSagaStorage();
            using (var readSession = await configuration.SynchronizedStorage.OpenSession(readContextBag))
            {
                SetActiveSagaInstance(readContextBag, new SagaWithoutCorrelationProperty(), new SagaWithoutCorrelationPropertyData { CorrelatedProperty = "whatever" });

                var result = await persister.Get<SagaWithoutCorrelationPropertyData>(sagaData.Id, readSession, readContextBag);
                Assert.AreEqual(sagaData.CorrelatedProperty, result.CorrelatedProperty);
            }
        }
    }
}