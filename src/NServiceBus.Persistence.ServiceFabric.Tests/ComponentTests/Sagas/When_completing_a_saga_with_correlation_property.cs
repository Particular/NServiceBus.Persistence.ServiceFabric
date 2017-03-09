namespace NServiceBus.Persistence.ComponentTests
{
    using System;
    using System.Threading.Tasks;
    using NUnit.Framework;
    
    [TestFixture]
    class When_completing_a_saga_with_correlation_property : SagaPersisterTests
    {
        [Test]
        public async Task Should_delete_the_saga()
        {
            var sagaId = Guid.NewGuid();

            var persister = configuration.SagaStorage;

            var insertContextBag = configuration.GetContextBagForSagaStorage();
            using (var savingSession = await configuration.SynchronizedStorage.OpenSession(insertContextBag))
            {
                var saga = new SagaWithCorrelationPropertyData { Id = sagaId, CorrelatedProperty = "whatever" };
                var correlationProperty = SetActiveSagaInstance(insertContextBag, new SagaWithCorrelationProperty(), saga);

                await persister.Save(saga, correlationProperty, savingSession, insertContextBag);
                await savingSession.CompleteAsync();
            }

            var intentionallySharedContext = configuration.GetContextBagForSagaStorage();
            SagaWithCorrelationPropertyData sagaData;
            using (var completeSession = await configuration.SynchronizedStorage.OpenSession(insertContextBag))
            {
                sagaData = await persister.Get<SagaWithCorrelationPropertyData>(nameof(SagaWithCorrelationPropertyData.CorrelatedProperty), "whatever", completeSession, intentionallySharedContext);
                SetActiveSagaInstance(intentionallySharedContext, new SagaWithCorrelationProperty(), sagaData);

                await persister.Complete(sagaData, completeSession, intentionallySharedContext );
                await completeSession.CompleteAsync();
            }

            SagaWithCorrelationPropertyData completedSaga;
            var readContextBag = configuration.GetContextBagForSagaStorage();
            using (var readSession = await configuration.SynchronizedStorage.OpenSession(readContextBag))
            {
                SetActiveSagaInstance(readContextBag, new SagaWithCorrelationProperty(), default(SagaWithCorrelationPropertyData));

                completedSaga = await persister.Get<SagaWithCorrelationPropertyData>(nameof(SagaWithCorrelationPropertyData.CorrelatedProperty), "whatever", readSession, readContextBag);
            }

            Assert.NotNull(sagaData);
            Assert.Null(completedSaga);
        }
    }
}
