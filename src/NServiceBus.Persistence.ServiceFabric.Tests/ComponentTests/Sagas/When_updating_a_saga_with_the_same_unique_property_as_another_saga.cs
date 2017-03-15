//namespace NServiceBus.Persistence.ComponentTests
//{
//    using System;
//    using System.Threading.Tasks;
//    using Extensibility;
//    using NUnit.Framework;
//
//    [TestFixture]
//    class When_updating_a_saga_with_the_same_unique_property_as_another_saga : SagaPersisterTests
//    {
//        [Test]
//        public async Task It_should_persist_successfully()
//        {
//            var saga1 = new SagaWithCorrelationPropertyData()
//            {
//                Id = Guid.NewGuid(),
//                FoundByFinderProperty = "whatever1"
//            };
//            var saga2 = new SagaWithCorrelationPropertyData
//            {
//                Id = Guid.NewGuid(),
//                FoundByFinderProperty = "whatever"
//            };
//
//            var persister = configuration.SagaStorage;
//            var session = new InMemorySynchronizedStorageSession();
//            await persister.Save(saga1, SagaMetadataHelper.GetMetadata<SagaWithCorrelationPropertyData>(saga1), session, new ContextBag());
//            await persister.Save(saga2, SagaMetadataHelper.GetMetadata<SagaWithCorrelationPropertyData>(saga2), session, new ContextBag());
//            await session.CompleteAsync();
//        }
//    }
//}