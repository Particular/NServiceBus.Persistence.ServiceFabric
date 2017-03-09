namespace NServiceBus.Persistence.ComponentTests
{
    using System;
    using System.Threading.Tasks;
    using Extensibility;
    using NUnit.Framework;
    
    [TestFixture]
    class When_completing_a_saga_with_unique_property : SagaPersisterTests
    {
        [Test]
        public async Task Should_delete_the_saga()
        {
            var saga = new SagaWithUniquePropertyData { Id = Guid.NewGuid(), UniqueString = "whatever" };

            var persister = configuration.SagaStorage;
            var insertContextBag = configuration.GetContextBagForSagaStorage();
            var correlationProperty = SetActiveSagaInstance(insertContextBag, new SagaWithUniqueProperty(), saga);

            using (var savingSession = await configuration.SynchronizedStorage.OpenSession(insertContextBag))
            {
                await persister.Save(saga, correlationProperty, savingSession, insertContextBag);
                await savingSession.CompleteAsync();
            }

            var intentionallySharedContext = configuration.GetContextBagForSagaStorage();
            SagaWithUniquePropertyData sagaData;
            using (var readSession = await configuration.SynchronizedStorage.OpenSession(insertContextBag))
            {
                sagaData = await persister.Get<SagaWithUniquePropertyData>(saga.Id, readSession, intentionallySharedContext);
            }

            using (var completeSession = await configuration.SynchronizedStorage.OpenSession(insertContextBag))
            {
                await persister.Complete(saga, completeSession, intentionallySharedContext );
                await completeSession.CompleteAsync();
            }

            SagaWithUniquePropertyData completedSaga;
            using (var readSession = await configuration.SynchronizedStorage.OpenSession(insertContextBag))
            {
                completedSaga = await persister.Get<SagaWithUniquePropertyData>(saga.Id, readSession, new ContextBag());
            }

            Assert.NotNull(sagaData);
            Assert.Null(completedSaga);
        }
    }
}
