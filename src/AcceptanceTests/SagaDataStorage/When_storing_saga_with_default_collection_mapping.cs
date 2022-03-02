namespace NServiceBus.Persistence.ServiceFabric.AcceptanceTests.SagaDataStorage
{
    using System;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using global::TestRunner;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NUnit.Framework;

    [TestFixture]
    public class When_storing_saga_with_default_collection_mapping : NServiceBusAcceptanceTest, INeed<IReliableStateManager>
    {
        IReliableStateManager stateManager;

        [Test]
        public async Task Should_use_saga_data_type_name()
        {
            var context = await Scenario.Define<Context>()
                .WithEndpoint<SagaEndpoint>(b => b.When(session => session.SendLocal(new StartSaga { SomeId = Guid.NewGuid() })))
                .Done(c => c.SagaId.HasValue)
                .Run(TimeSpan.FromSeconds(20));

            var collection = await stateManager.GetOrAddAsync<IReliableDictionary<Guid, SagaEntry>>(nameof(SagaEndpoint.ConventionBasedSaga.ConventionBasedSagaData));

            using (var tx = stateManager.CreateTransaction())
            {
                var conditionalValue = await collection.TryGetValueAsync(tx, context.SagaId.Value);

                Assert.True(conditionalValue.HasValue);
            }
        }

        public void Need(IReliableStateManager stateManager)
        {
            this.stateManager = stateManager;
        }

        public class Context : ScenarioContext
        {
            public Guid? SagaId { get; set; }
        }

        public class SagaEndpoint : EndpointConfigurationBuilder
        {
            public SagaEndpoint()
            {
                EndpointSetup<DefaultServer>();
            }

            public class ConventionBasedSaga : Saga<ConventionBasedSaga.ConventionBasedSagaData>, IAmStartedByMessages<StartSaga>
            {
                Context testContext;

                public ConventionBasedSaga(Context testContext)
                {
                    this.testContext = testContext;
                }

                protected override void ConfigureHowToFindSaga(SagaPropertyMapper<ConventionBasedSagaData> mapper)
                {
                    mapper.ConfigureMapping<StartSaga>(m => m.SomeId).ToSaga(d => d.SomeId);
                }

                public Task Handle(StartSaga message, IMessageHandlerContext context)
                {
                    Data.SomeId = message.SomeId;
                    testContext.SagaId = ((IContainSagaData)Data).Id;

                    return Task.FromResult(0);
                }

                public class ConventionBasedSagaData : ContainSagaData
                {
                    public Guid SomeId { get; set; }
                }
            }
        }

        public class StartSaga : IMessage
        {
            public Guid SomeId { get; set; }
        }
    }
}