namespace NServiceBus.Persistence.ComponentTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class When_persisting_a_saga_with_complex_types : SagaPersisterTests
    {
        [Test]
        public async Task It_should_get_deep_copy()
        {
            var correlationPropertyData = Guid.NewGuid().ToString();
            var sagaData = new SagaWithComplexTypeEntity { Ints = new List<int> { 1, 2 }, CorrelationProperty = correlationPropertyData };

            var persister = configuration.SagaStorage;
            var savingContextBag = configuration.GetContextBagForSagaStorage();
            Guid generatedSagaId;
            using (var session = await configuration.SynchronizedStorage.OpenSession(savingContextBag))
            {
                var correlationProperty = SetActiveSagaInstance(savingContextBag, new SagaWithComplexType(), sagaData);
                generatedSagaId = sagaData.Id;

                await persister.Save(sagaData, correlationProperty, session, savingContextBag);
                await session.CompleteAsync();
            }

            var readingContextBag = configuration.GetContextBagForSagaStorage();
            using (var session = await configuration.SynchronizedStorage.OpenSession(savingContextBag))
            {
                SetActiveSagaInstance(readingContextBag, new SagaWithComplexType(), sagaData);
                var retrieved = await persister.Get<SagaWithComplexTypeEntity>(generatedSagaId, session, readingContextBag);
                SetActiveSagaInstance(readingContextBag, new SagaWithComplexType(), retrieved);

                CollectionAssert.AreEqual(sagaData.Ints, retrieved.Ints);
                Assert.False(ReferenceEquals(sagaData.Ints, retrieved.Ints));
            }
        }
    }
}