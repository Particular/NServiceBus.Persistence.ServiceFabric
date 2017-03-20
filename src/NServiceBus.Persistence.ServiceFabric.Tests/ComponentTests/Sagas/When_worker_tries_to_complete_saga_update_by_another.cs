namespace NServiceBus.Persistence.ComponentTests
{
    using System;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class When_worker_tries_to_complete_saga_update_by_another : SagaPersisterTests
    {
        [Test]
        public async Task Should_fail()
        {
            var correlationPropertyData = Guid.NewGuid().ToString();
            var saga = new TestSagaData { SomeId = correlationPropertyData };

            var persister = configuration.SagaStorage;
            var insertContextBag = configuration.GetContextBagForSagaStorage();
            using (var insertSession = await configuration.SynchronizedStorage.OpenSession(insertContextBag))
            {
                var correlationProperty = SetActiveSagaInstance(insertContextBag, new TestSaga(), saga);

                await persister.Save(saga, correlationProperty, insertSession, insertContextBag);
                await insertSession.CompleteAsync();
            }

            var winningContext = configuration.GetContextBagForSagaStorage();
            var winningSaveSession = await configuration.SynchronizedStorage.OpenSession(winningContext);
            SetActiveSagaInstance(winningContext, new TestSaga(), saga);
            var record = await persister.Get<TestSagaData>(saga.Id, winningSaveSession, winningContext);
            SetActiveSagaInstance(winningContext, new TestSaga(), record);

            var losingContext = configuration.GetContextBagForSagaStorage();
            var losingSaveSession = await configuration.SynchronizedStorage.OpenSession(losingContext);
            SetActiveSagaInstance(losingContext, new TestSaga(), saga);
            var staleRecord = await persister.Get<TestSagaData>("SomeId", correlationPropertyData, losingSaveSession, losingContext);
            SetActiveSagaInstance(losingContext, new TestSaga(), staleRecord);

            record.DateTimeProperty = DateTime.Now;
            await persister.Update(record, winningSaveSession, winningContext);
            await winningSaveSession.CompleteAsync();
            winningSaveSession.Dispose();

            await persister.Complete(staleRecord, losingSaveSession, losingContext);
            Assert.That(async () => await losingSaveSession.CompleteAsync(), Throws.InstanceOf<Exception>().And.Message.EqualTo("Saga can't be completed as it was updated by another process."));
        }


    }
}