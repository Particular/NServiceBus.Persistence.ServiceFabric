namespace NServiceBus.Persistence.ComponentTests
{
    using System;
    using System.Threading.Tasks;
    using Extensibility;
    using NUnit.Framework;

    [TestFixture]
    class When_completing_a_saga : SagaPersisterTests
    {
        [Test]
        public async Task Should_delete_the_saga()
        {
            var saga = new TestSagaData
            {
                Id = Guid.NewGuid()
            };

            var persister = configuration.SagaStorage;
            var insertContextBag = configuration.GetContextBagForSagaStorage();
            var correlationProperty = SetActiveSagaInstance(insertContextBag, new TestSaga(), saga);

            using (var insertSession = await configuration.SynchronizedStorage.OpenSession(insertContextBag))
            {
                await persister.Save(saga, correlationProperty, insertSession, new ContextBag());
                await insertSession.CompleteAsync();
            }

            var intentionallySharedContext = configuration.GetContextBagForSagaStorage();
            TestSagaData sagaData;
            using (var readSession = await configuration.SynchronizedStorage.OpenSession(insertContextBag))
            {
                sagaData = await persister.Get<TestSagaData>(saga.Id, readSession, intentionallySharedContext);
            }

            using (var deleteSession = await configuration.SynchronizedStorage.OpenSession(insertContextBag))
            {
                await persister.Complete(saga, deleteSession, intentionallySharedContext );
                await deleteSession.CompleteAsync();
            }

            TestSagaData completedSaga;
            using (var readSession = await configuration.SynchronizedStorage.OpenSession(insertContextBag))
            {
                completedSaga = await persister.Get<TestSagaData>(saga.Id, readSession, configuration.GetContextBagForSagaStorage());
            }

            Assert.NotNull(sagaData);
            Assert.Null(completedSaga);
        }
    }
}