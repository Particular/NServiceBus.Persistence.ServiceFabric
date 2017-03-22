namespace NServiceBus.Persistence.ComponentTests
{
    using System;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class When_updating_a_saga_with_the_same_unique_property_value : SagaPersisterTests<SagaWithCorrelationProperty, SagaWithCorrelationPropertyData>
    {
        [Test]
        public async Task It_should_persist_successfully()
        {
            var correlationPropertyData = Guid.NewGuid().ToString();
            var saga1 = new SagaWithCorrelationPropertyData
            {
                CorrelatedProperty = correlationPropertyData
            };

            await SaveSaga(saga1);

            await GetByCorrelationPropertyAndUpdate(nameof(SagaWithCorrelationPropertyData.CorrelatedProperty), correlationPropertyData, _ => { });
        }
    }
}