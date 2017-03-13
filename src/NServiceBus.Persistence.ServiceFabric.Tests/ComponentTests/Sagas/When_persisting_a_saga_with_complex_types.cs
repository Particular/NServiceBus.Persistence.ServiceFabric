﻿namespace NServiceBus.Persistence.ComponentTests
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
            var sagaData = new SagaWithComplexType
            {
                Id = Guid.NewGuid(),
                Ints = new List<int> { 1, 2 }
            };

            var persister = configuration.SagaStorage;
            var savingContextBag = configuration.GetContextBagForSagaStorage();
            using (var session = await configuration.SynchronizedStorage.OpenSession(savingContextBag))
            {
                await persister.Save(sagaData, null, session, savingContextBag);
                await session.CompleteAsync();
            }

            var readingContextBag = configuration.GetContextBagForSagaStorage();
            using (var session = await configuration.SynchronizedStorage.OpenSession(savingContextBag))
            {
                var retrieved = await persister.Get<SagaWithComplexType>(sagaData.Id, session, readingContextBag);

                CollectionAssert.AreEqual(sagaData.Ints, retrieved.Ints);
                Assert.False(ReferenceEquals(sagaData.Ints, retrieved.Ints));
            }
        }

        class SagaWithComplexType : ContainSagaData
        {
            public List<int> Ints { get; set; }
        }
    }
}