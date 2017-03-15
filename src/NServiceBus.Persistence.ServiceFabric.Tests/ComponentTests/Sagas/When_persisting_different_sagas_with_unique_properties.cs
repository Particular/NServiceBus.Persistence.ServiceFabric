namespace NServiceBus.Persistence.ComponentTests
{
    using System;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class When_persisting_different_sagas_with_unique_properties : SagaPersisterTests
    {
        [Test]
        public async Task It_should_persist_successfully()
        {
            var saga1Id = Guid.NewGuid();
            var saga1 = new SagaWithCorrelationPropertyData
            {
                Id = saga1Id,
                CorrelatedProperty = saga1Id.ToString()
            };
            var saga2 = new AnotherSagaWithCorrelatedPropertyData
            {
                Id = Guid.NewGuid(),
                CorrelatedProperty = saga1Id.ToString()
            };

            var persister = configuration.SagaStorage;
            var savingContextBag = configuration.GetContextBagForSagaStorage();
            using (var session = await configuration.SynchronizedStorage.OpenSession(savingContextBag))
            {
                var correlationPropertySaga1 = SetActiveSagaInstance(savingContextBag, new SagaWithCorrelationProperty(), saga1);
                await persister.Save(saga1, correlationPropertySaga1, session, savingContextBag);

                var correlationPropertySaga2 = SetActiveSagaInstance(savingContextBag, new AnotherSagaWithCorrelatedProperty(), saga2);
                await persister.Save(saga2, correlationPropertySaga2, session, savingContextBag);

                await session.CompleteAsync();
            }
        }
    }
}