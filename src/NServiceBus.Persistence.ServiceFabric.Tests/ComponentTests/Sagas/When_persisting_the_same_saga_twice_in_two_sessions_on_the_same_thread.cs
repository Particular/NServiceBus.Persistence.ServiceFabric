namespace NServiceBus.Persistence.ComponentTests
{
    using System;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class When_persisting_the_same_saga_twice_in_two_sessions_on_the_same_thread : SagaPersisterTests<TestSaga, TestSagaData>
    {
        [Test]
        public async Task Save_throws_concurrency_violation()
        {
            var correlationPropertyData = Guid.NewGuid().ToString();
            var saga = new TestSagaData { SomeId = correlationPropertyData};

            await SaveSaga(saga);
            var persister = configuration.SagaStorage;
            
            TestSagaData returnedSaga1;
            var readContextBag = configuration.GetContextBagForSagaStorage();
            using (var readSession = await configuration.SynchronizedStorage.OpenSession(readContextBag))
            {
                SetActiveSagaInstanceForGet<TestSaga, TestSagaData>(readContextBag, new TestSagaData());

                returnedSaga1 = await persister.Get<TestSagaData>(saga.Id, readSession, readContextBag);

                await readSession.CompleteAsync();
            }

            var winningContext = configuration.GetContextBagForSagaStorage();
            var winningSaveSession = await configuration.SynchronizedStorage.OpenSession(winningContext);

            returnedSaga1.DateTimeProperty = DateTime.Now;
            SetActiveSagaInstanceForGet<TestSaga, TestSagaData>(winningContext, returnedSaga1);
            await persister.Update(returnedSaga1, winningSaveSession, readContextBag);
            await winningSaveSession.CompleteAsync();
            winningSaveSession.Dispose();

            var losingContext = configuration.GetContextBagForSagaStorage();
            var losingSaveSession = await configuration.SynchronizedStorage.OpenSession(losingContext);
            SetActiveSagaInstanceForGet<TestSaga, TestSagaData>(losingContext, returnedSaga1);
            await persister.Update(returnedSaga1, losingSaveSession, readContextBag);

            Assert.That(async () => await losingSaveSession.CompleteAsync(), Throws.InstanceOf<Exception>().And.Message.EndWith($"concurrency violation: saga entity Id[{saga.Id}] already saved."));
        }

        [Test]
        public async Task Save_process_is_repeatable()
        {
            var correlationPropertyData = Guid.NewGuid().ToString();
            var saga = new TestSagaData { SomeId = correlationPropertyData };

            var persister = configuration.SagaStorage;
            var insertContextBag = configuration.GetContextBagForSagaStorage();
            using (var insertSession = await configuration.SynchronizedStorage.OpenSession(insertContextBag))
            {
                var correlationProperty = SetActiveSagaInstanceForSave(insertContextBag, new TestSaga(), saga);

                await persister.Save(saga, correlationProperty, insertSession, insertContextBag);
                await insertSession.CompleteAsync();
            }

            var winningContext1 = configuration.GetContextBagForSagaStorage();
            var winningSaveSession1 = await configuration.SynchronizedStorage.OpenSession(winningContext1);
            SetActiveSagaInstanceForGet<TestSaga, TestSagaData>(winningContext1, saga);
            var record1 = await persister.Get<TestSagaData>(saga.Id, winningSaveSession1, winningContext1);

            var losingContext1 = configuration.GetContextBagForSagaStorage();
            var losingSaveSession1 = await configuration.SynchronizedStorage.OpenSession(losingContext1);
            SetActiveSagaInstanceForGet<TestSaga, TestSagaData>(losingContext1, saga);
            var staleRecord1 = await persister.Get<TestSagaData>("SomeId", correlationPropertyData, losingSaveSession1, losingContext1);
            SetActiveSagaInstanceForGet<TestSaga, TestSagaData>(losingContext1, staleRecord1);

            record1.DateTimeProperty = DateTime.Now;
            await persister.Update(record1, winningSaveSession1, winningContext1);
            await winningSaveSession1.CompleteAsync();
            winningSaveSession1.Dispose();

            await persister.Update(staleRecord1, losingSaveSession1, losingContext1);
            Assert.That(async () => await losingSaveSession1.CompleteAsync(), Throws.InstanceOf<Exception>().And.Message.EndsWith($"concurrency violation: saga entity Id[{saga.Id}] already saved."));
            losingSaveSession1.Dispose();

            var winningContext2 = configuration.GetContextBagForSagaStorage();
            var winningSaveSession2 = await configuration.SynchronizedStorage.OpenSession(winningContext2);
            SetActiveSagaInstanceForGet<TestSaga, TestSagaData>(winningContext2, saga);
            var record2 = await persister.Get<TestSagaData>(saga.Id, winningSaveSession2, winningContext2);
            SetActiveSagaInstanceForGet<TestSaga, TestSagaData>(winningContext2, record2);

            var losingContext2 = configuration.GetContextBagForSagaStorage();
            var losingSaveSession2 = await configuration.SynchronizedStorage.OpenSession(losingContext2);
            SetActiveSagaInstanceForGet<TestSaga, TestSagaData>(losingContext2, saga);
            var staleRecord2 = await persister.Get<TestSagaData>("SomeId", correlationPropertyData, losingSaveSession2, losingContext2);
            SetActiveSagaInstanceForGet<TestSaga, TestSagaData>(losingContext2, staleRecord2);

            record2.DateTimeProperty = DateTime.Now;
            await persister.Update(record2, winningSaveSession2, winningContext2);
            await winningSaveSession2.CompleteAsync();
            winningSaveSession2.Dispose();

            await persister.Update(staleRecord2, losingSaveSession2, losingContext2);
            Assert.That(async () => await losingSaveSession2.CompleteAsync(), Throws.InstanceOf<Exception>().And.Message.EndsWith($"concurrency violation: saga entity Id[{saga.Id}] already saved."));
            losingSaveSession2.Dispose();
        }
    }
}