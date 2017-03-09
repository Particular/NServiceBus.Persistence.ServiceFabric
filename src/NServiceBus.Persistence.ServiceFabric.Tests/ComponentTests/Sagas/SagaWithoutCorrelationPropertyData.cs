namespace NServiceBus.Persistence.ComponentTests
{
    using System;
    using System.Threading.Tasks;

    class SagaWithoutCorrelationProperty : Saga<SagaWithoutCorrelationPropertyData>, IAmStartedByMessages<SagaWithoutCorrelationPropertyStartingMessage>
    {
        public Task Handle(SagaWithoutCorrelationPropertyStartingMessage message, IMessageHandlerContext context)
        {
            throw new NotImplementedException();
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<SagaWithoutCorrelationPropertyData> mapper)
        {
            //not implemented
        }
    }
    public class SagaWithoutCorrelationPropertyData : ContainSagaData
    {
        public string NonUniqueString { get; set; }
    }

    class SagaWithoutCorrelationPropertyStartingMessage
    {
        public string NonUniqueString { get; set; }
    }
}