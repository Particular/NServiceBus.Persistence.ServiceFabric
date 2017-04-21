namespace NServiceBus.Persistence.ComponentTests
{
    using System;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class When_worker_tries_to_complete_saga_update_by_another : SagaPersisterTests<TestSaga,TestSagaData>
    {
        [Test]
        public async Task Should_fail()
        {
            var correlationPropertyData = Guid.NewGuid().ToString();
            var saga = new TestSagaData { SomeId = correlationPropertyData, DateTimeProperty = DateTime.UtcNow };

            await SaveSaga(saga);

            var persister = configuration.SagaStorage;

            var winningContext = configuration.GetContextBagForSagaStorage();
            var winningSaveSession = await configuration.SynchronizedStorage.OpenSession(winningContext);
            SetActiveSagaInstanceForGet<TestSaga, TestSagaData>(winningContext, saga);
            var record = await persister.Get<TestSagaData>(saga.Id, winningSaveSession, winningContext);
            SetActiveSagaInstanceForGet<TestSaga, TestSagaData>(winningContext, record);

            var losingContext = configuration.GetContextBagForSagaStorage();
            var losingSaveSession = await configuration.SynchronizedStorage.OpenSession(losingContext);
            SetActiveSagaInstanceForGet<TestSaga, TestSagaData>(losingContext, saga);
            var staleRecord = await persister.Get<TestSagaData>("SomeId", correlationPropertyData, losingSaveSession, losingContext);
            SetActiveSagaInstanceForGet<TestSaga, TestSagaData>(losingContext, staleRecord);

            record.DateTimeProperty = DateTime.UtcNow;
            await persister.Update(record, winningSaveSession, winningContext);
            await winningSaveSession.CompleteAsync();
            winningSaveSession.Dispose();

            await persister.Complete(staleRecord, losingSaveSession, losingContext);
            Assert.That(async () => await losingSaveSession.CompleteAsync(), Throws.InstanceOf<Exception>().And.Message.EqualTo("Saga can't be completed as it was updated by another process."));
        }
    }
}