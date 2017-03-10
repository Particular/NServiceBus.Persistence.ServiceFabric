namespace NServiceBus.Persistence.ComponentTests
{
    using System;
    using System.Threading.Tasks;

    class SagaWithoutCorrelationProperty : Saga<SagaWithoutCorrelationPropertyData>, 
        IAmStartedByMessages<SagaWithoutCorrelationPropertyStartingMessage>
    {
        public Task Handle(SagaWithoutCorrelationPropertyStartingMessage message, IMessageHandlerContext context)
        {
            throw new NotImplementedException();
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<SagaWithoutCorrelationPropertyData> mapper)
        {
            mapper.ConfigureMapping<SagaWithoutCorrelationPropertyStartingMessage>(m => m.CorrelatedProperty).ToSaga(s => s.CorrelatedProperty);
        }
    }

    public class SagaWithoutCorrelationPropertyData : ContainSagaData
    {
        public string CorrelatedProperty { get; set; }
    }

    public class SagaWithoutCorrelationPropertyStartingMessage : IMessage
    {
        public string CorrelatedProperty { get; set; }
    }
}