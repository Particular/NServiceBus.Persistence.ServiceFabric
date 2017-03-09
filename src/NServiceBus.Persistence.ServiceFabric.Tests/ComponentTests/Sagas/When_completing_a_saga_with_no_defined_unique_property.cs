namespace NServiceBus.Persistence.ComponentTests
{
    using System;
    using System.Threading.Tasks;
    using Extensibility;
    using NUnit.Framework;
    using Sagas;

    [TestFixture]
    public class When_completing_a_saga_with_no_defined_unique_property : SagaPersisterTests
    {
        /// <summary>
        /// There can be a saga that is only started by a message and then is driven by timeouts only. 
        /// This kind of saga would not require to be correlated by any property. This test ensures that in-memory persistence covers this case and can handle this kind of sagas properly. 
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task It_should_successfully_remove_the_saga()
        {
            var id = Guid.NewGuid();
            var sagaData = new SagaWithoutUniquePropertyData
            {
                Id = id,
                NonUniqueString = "whatever"
            };

            var persister = configuration.SagaStorage;
            var savingContextBag = configuration.GetContextBagForSagaStorage();
            SetActiveSagaInstance(savingContextBag, sagaData);

            var savingSession = await configuration.SynchronizedStorage.OpenSession(savingContextBag);

            await persister.Save(sagaData, null, savingSession, savingContextBag);
            await savingSession.CompleteAsync();

            // second session
            var completingContextBag = configuration.GetContextBagForSagaStorage();
            var completingSession = await configuration.SynchronizedStorage.OpenSession(completingContextBag);

            var saga = await persister.Get<SagaWithoutUniquePropertyData>(id, completingSession, completingContextBag);
            SetActiveSagaInstance(completingContextBag, saga);
            await persister.Complete(saga, completingSession, completingContextBag);
            await completingSession.CompleteAsync();

            var result = await persister.Get<SagaWithoutUniquePropertyData>(sagaData.Id, savingSession, configuration.GetContextBagForSagaStorage());

            Assert.That(result, Is.Null);
        }

        static void SetActiveSagaInstance(ContextBag savingContextBag, SagaWithoutUniquePropertyData sagaData)
        {
            var sagaInstance = new ActiveSagaInstance(new SagaWithoutUniqueProperty(), SagaMetadata.Create(typeof(SagaWithoutUniqueProperty)), () => DateTime.UtcNow);
            sagaInstance.AttachNewEntity(sagaData);
            savingContextBag.Set(sagaInstance);
        }
    }
}