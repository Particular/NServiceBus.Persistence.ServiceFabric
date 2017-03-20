namespace NServiceBus.Persistence.ComponentTests
{
    using System;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class When_multiple_workers_retrieve_same_saga_on_the_same_thread : SagaPersisterTests
    {
        [Test]
        public async Task Save_fails_when_data_changes_between_read_and_update()
        {
            var correlationPropertyData = Guid.NewGuid().ToString();

            var persister = configuration.SagaStorage;
            var insertContextBag = configuration.GetContextBagForSagaStorage();
            Guid generatedSagaId;
            using (var insertSession = await configuration.SynchronizedStorage.OpenSession(insertContextBag))
            {
                var sagaData = new TestSagaData { SomeId = correlationPropertyData };
                var correlationProperty = SetActiveSagaInstance(insertContextBag, new TestSaga(), sagaData);
                generatedSagaId = sagaData.Id;

                await persister.Save(sagaData, correlationProperty, insertSession, insertContextBag);
                await insertSession.CompleteAsync();
            }

            var winningContext = configuration.GetContextBagForSagaStorage();
            var winningSaveSession = await configuration.SynchronizedStorage.OpenSession(winningContext);
            var record = await persister.Get<TestSagaData>(generatedSagaId, winningSaveSession, winningContext);

            var losingContext = configuration.GetContextBagForSagaStorage();
            var losingSaveSession = await configuration.SynchronizedStorage.OpenSession(losingContext);
            var staleRecord = await persister.Get<TestSagaData>("SomeId", correlationPropertyData, losingSaveSession, losingContext);

            record.DateTimeProperty = DateTime.Now;
            await persister.Update(record, winningSaveSession, winningContext);
            await winningSaveSession.CompleteAsync();
            winningSaveSession.Dispose();

            await persister.Update(staleRecord, losingSaveSession, losingContext);
            Assert.That(async () => await losingSaveSession.CompleteAsync(), Throws.InstanceOf<Exception>().And.Message.EndsWith($"concurrency violation: saga entity Id[{generatedSagaId}] already saved."));
        }
    }
}