namespace NServiceBus.Persistence.ComponentTests
{
    using System;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    class When_persisting_a_saga_with_the_same_unique_property_as_another_saga: SagaPersisterTests
    {
        [Test]
        public async Task It_should_enforce_uniqueness()
        {
            var saga1 = new SagaWithCorrelationPropertyData
            {
                Id = Guid.NewGuid(),
                CorrelatedProperty = "whatever"
            };
            var saga2 = new SagaWithCorrelationPropertyData
            {
                Id = Guid.NewGuid(),
                CorrelatedProperty = "whatever"
            };

            var persister = configuration.SagaStorage;
            var winningContextBag = configuration.GetContextBagForSagaStorage();
            var winningSession = await configuration.SynchronizedStorage.OpenSession(winningContextBag);
            var losingContextBag = configuration.GetContextBagForSagaStorage();
            var losingSession = await configuration.SynchronizedStorage.OpenSession(losingContextBag);

            var correlationPropertySaga1 = SetActiveSagaInstance(winningContextBag, new SagaWithCorrelationProperty(), saga1);
            await persister.Save(saga1, correlationPropertySaga1, winningSession, winningContextBag);
            var correlationPropertySaga2 = SetActiveSagaInstance(losingContextBag, new SagaWithCorrelationProperty(), saga2);
            await persister.Save(saga2, correlationPropertySaga2, losingSession, losingContextBag);

            await winningSession.CompleteAsync();

            Assert.That(async () => await losingSession.CompleteAsync(), Throws.InstanceOf<InvalidOperationException>());
        }
    }
}