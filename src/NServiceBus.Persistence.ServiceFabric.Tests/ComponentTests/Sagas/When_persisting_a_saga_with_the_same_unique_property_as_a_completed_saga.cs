namespace NServiceBus.Persistence.ComponentTests
{
    using System;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using Sagas;

    [TestFixture]
    public class When_persisting_a_saga_with_the_same_unique_property_as_a_completed_saga : SagaPersisterTests
    {
        [Test]
        public async Task It_should_persist_successfully()
        {
            var persister = configuration.SagaStorage;

            var correlationPropertyData = Guid.NewGuid().ToString();
            var saga1 = new SagaWithCorrelationPropertyData { CorrelatedProperty = correlationPropertyData };
            var saga2 = new SagaWithCorrelationPropertyData { CorrelatedProperty = correlationPropertyData };

            await SaveSaga(persister, saga1);
            await CompleteSaga(persister, saga1.Id);

            await SaveSaga(persister, saga2);
            await CompleteSaga(persister, saga2.Id);

            await SaveSaga(persister, saga1);
            await CompleteSaga(persister, saga1.Id);
        }

        async Task SaveSaga(ISagaPersister persister, SagaWithCorrelationPropertyData saga)
        {
            var savingContextBag = configuration.GetContextBagForSagaStorage();
            using (var session = await configuration.SynchronizedStorage.OpenSession(savingContextBag))
            {
                var correlationProperty = SetActiveSagaInstance(savingContextBag, new SagaWithCorrelationProperty(), saga);

                await persister.Save(saga, correlationProperty, session, savingContextBag);
                await session.CompleteAsync();
            }
        }

        async Task CompleteSaga(ISagaPersister persister, Guid sagaId)
        {
            var completionContextBag = configuration.GetContextBagForSagaStorage();
            using (var session = await configuration.SynchronizedStorage.OpenSession(completionContextBag))
            {
                var saga = await persister.Get<SagaWithCorrelationPropertyData>(sagaId, session, completionContextBag);
                await persister.Complete(saga, session, completionContextBag);
                await session.CompleteAsync();
            }
        }
    }
}