namespace NServiceBus.Persistence.ComponentTests
{
    using System;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class When_persisting_different_sagas_with_no_defined_unique_properties : SagaPersisterTests
    {
        [Test]
        public async Task It_should_persist_successfully()
        {
            var saga1Id = Guid.NewGuid();
            var saga1 = new SagaWithoutCorrelationPropertyData
            {
                Id = saga1Id,
                CorrelatedProperty = saga1Id.ToString()
            };
            var saga2 = new AnotherSagaWithoutCorrelationPropertyData
            {
                Id = Guid.NewGuid(),
                CorrelatedProperty = saga1Id.ToString()
            };

            var persister = configuration.SagaStorage;
            var savingContextBag = configuration.GetContextBagForSagaStorage();
            using (var session = await configuration.SynchronizedStorage.OpenSession(savingContextBag))
            {
                var correlationPropertySaga1 = SetActiveSagaInstance(savingContextBag, new SagaWithoutCorrelationProperty(), saga1, typeof(CustomFinder));
                await persister.Save(saga1, correlationPropertySaga1, session, savingContextBag);

                var correlationPropertySaga2 = SetActiveSagaInstance(savingContextBag, new AnotherSagaWithoutCorrelationProperty(), saga2, typeof(AnotherCustomFinder));
                await persister.Save(saga2, correlationPropertySaga2, session, savingContextBag);

                await session.CompleteAsync();
            }
        }
    }
}