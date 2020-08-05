namespace NServiceBus.Persistence.ComponentTests
{
    using System;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class When_concurrent_update_exceed_transaction_timeout_pessimistic : SagaPersisterTests<TestSaga,TestSagaData>
    {
        public override async Task OneTimeSetUp()
        {
            configuration = new PersistenceTestsConfiguration(TimeSpan.FromMilliseconds(20));
            await configuration.Configure();
        }

        [Test]
        public async Task Should_fail_with_timeout()
        {
            configuration.RequiresPessimisticConcurrencySupport();

            var correlationPropertyData = Guid.NewGuid().ToString();
            var saga = new TestSagaData { SomeId = correlationPropertyData, DateTimeProperty = DateTime.UtcNow };
            await SaveSaga(saga);

            var firstSessionDateTimeValue = DateTime.UtcNow.AddDays(-2);
            var secondSessionDateTimeValue = DateTime.UtcNow.AddDays(-1);

            var firstSessionGetDone = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var secondSessionGetDone = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var persister = configuration.SagaStorage;

            async Task FirstSession()
            {
                var firstSessionContext = configuration.GetContextBagForSagaStorage();
                using (var firstSaveSession = await configuration.SynchronizedStorage.OpenSession(firstSessionContext))
                {
                    var record = await persister.Get<TestSagaData>(saga.Id, firstSaveSession, firstSessionContext);
                    firstSessionGetDone.SetResult(true);

                    await Task.Delay(1000).ConfigureAwait(false);
                    await secondSessionGetDone.Task.ConfigureAwait(false);

                    record.DateTimeProperty = firstSessionDateTimeValue;
                    await persister.Update(record, firstSaveSession, firstSessionContext);
                    await firstSaveSession.CompleteAsync();
                }
            }

            async Task SecondSession()
            {
                var secondContext = configuration.GetContextBagForSagaStorage();
                using (var secondSession = await configuration.SynchronizedStorage.OpenSession(secondContext))
                {
                    await firstSessionGetDone.Task.ConfigureAwait(false);

                    var recordTask = persister.Get<TestSagaData>(saga.Id, secondSession, secondContext);
                    secondSessionGetDone.SetResult(true);

                    var record = await recordTask.ConfigureAwait(false);
                    record.DateTimeProperty = secondSessionDateTimeValue;
                    await persister.Update(record, secondSession, secondContext);
                    await secondSession.CompleteAsync();
                }
            }

            var firstSessionTask = FirstSession();
            var secondSessionTask = SecondSession();

            Assert.DoesNotThrowAsync(async () => await firstSessionTask);
            Assert.CatchAsync<Exception>(async () => await secondSessionTask);

            var updatedSaga = await GetById(saga.Id);
            Assert.That(updatedSaga.DateTimeProperty, Is.EqualTo(firstSessionDateTimeValue));
        }
    }
}