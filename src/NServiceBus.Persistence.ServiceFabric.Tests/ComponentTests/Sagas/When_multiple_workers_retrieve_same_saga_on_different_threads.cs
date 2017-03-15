//namespace NServiceBus.Persistence.ComponentTests
//{
//    using System;
//    using System.Threading.Tasks;
//    using NUnit.Framework;
//
//    [TestFixture]
//    public class When_multiple_workers_retrieve_same_saga_on_different_threads : SagaPersisterTests
//    {
//        [Test]
//        public async Task Save_fails_when_data_changes_between_read_and_update_on_same_thread()
//        {
//            var sagaId = Guid.NewGuid();
//            var saga = new TestSagaData { Id = sagaId, SomeId = sagaId.ToString() };
//
//            var persister = configuration.SagaStorage;
//            var insertContextBag = configuration.GetContextBagForSagaStorage();
//            using (var insertSession = await configuration.SynchronizedStorage.OpenSession(insertContextBag))
//            {
//                var correlationProperty = SetActiveSagaInstance(insertContextBag, new TestSaga(), saga);
//
//                await persister.Save(saga, correlationProperty, insertSession, insertContextBag);
//                await insertSession.CompleteAsync();
//            }
//
//
//            var tcs = new TaskCompletionSource<bool>();
//            var startSync = new TaskCompletionSource<bool>();
//
//            var t1 = Task.Run(async () =>
//            {
//                var winningContext = configuration.GetContextBagForSagaStorage();
//                var winningSaveSession = await configuration.SynchronizedStorage.OpenSession(winningContext);
//                var record = await persister.Get<TestSagaData>(saga.Id, winningSaveSession, winningContext);
//                winningSaveSession.Dispose();
//
//                startSync.SetResult(true);
//                await tcs.Task;
//
//                winningContext = configuration.GetContextBagForSagaStorage();
//                winningSaveSession = await configuration.SynchronizedStorage.OpenSession(winningContext);
//                record.DateTimeProperty = DateTime.Now;
//                await persister.Update(record, winningSaveSession, winningContext);
//                await winningSaveSession.CompleteAsync();
//                winningSaveSession.Dispose();
//
//            });
//
//            var t2 = Task.Run(async () =>
//            {
//                await startSync.Task;
//
//                var losingTransaction = stateManager.CreateTransaction();
//                await primary.TryGetValueAsync(losingTransaction, "Key");
//                await losingTransaction.CommitAsync();
//                losingTransaction.Dispose();
//
//                tcs.SetResult(true);
//                await t1;
//
//                losingTransaction = stateManager.CreateTransaction();
//                var result = await primary.TryUpdateAsync(losingTransaction, "Key", "Value2", "Value");
//                Console.WriteLine($"Result t2 {result}");
//                losingTransaction.Dispose();
//                Assert.IsFalse(result, "Expected to fail to update the value, but didn't.");
//
//                transaction = stateManager.CreateTransaction();
//                var conditional = await primary.TryGetValueAsync(transaction, "Key");
//                transaction.Dispose();
//
//                Assert.AreEqual("Value1", conditional.Value);
//            });
//
//            await t2;
//
//
//
//
//
//
//            var losingContext = configuration.GetContextBagForSagaStorage();
//            var losingSaveSession = await configuration.SynchronizedStorage.OpenSession(losingContext);
//            var staleRecord = await persister.Get<TestSagaData>("SomeId", sagaId.ToString(), losingSaveSession, losingContext);
//
//
//
//            await persister.Update(staleRecord, losingSaveSession, losingContext);
//            Assert.That(async () => await losingSaveSession.CompleteAsync(), Throws.InstanceOf<Exception>().And.Message.EndsWith($"concurrency violation: saga entity Id[{saga.Id}] already saved."));
//        }
//    }
//}