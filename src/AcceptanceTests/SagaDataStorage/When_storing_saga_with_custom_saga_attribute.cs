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
    public class When_storing_saga_with_custom_saga_attribute : NServiceBusAcceptanceTest, INeed<IReliableStateManager>
    {
        const string CustomCollectionName = "custom-collection-name";
        const string CustomEntityName = "custom-entity-name";

        IReliableStateManager stateManager;

        [Test]
        public async Task Should_use_custom_collection_name()
        {
            var context = await Scenario.Define<Context>()
                .WithEndpoint<SagaEndpoint>(b => b.When(session => session.SendLocal(new StartSaga { SomeId = Guid.NewGuid() })))
                .Done(c => c.SagaId.HasValue)
                .Run(TimeSpan.FromSeconds(20));

            var collection = await stateManager.GetOrAddAsync<IReliableDictionary<Guid, SagaEntry>>(CustomCollectionName);

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

            [ServiceFabricSaga(CollectionName = CustomCollectionName, SagaDataName = CustomEntityName)]
            public class CustomizedCollectionSaga : Saga<CustomizedCollectionSaga.CustomizedCollectionSagaData>, IAmStartedByMessages<StartSaga>
            {
                Context testContext;

                public CustomizedCollectionSaga(Context testContext)
                {
                    this.testContext = testContext;
                }

                protected override void ConfigureHowToFindSaga(SagaPropertyMapper<CustomizedCollectionSagaData> mapper)
                {
                    mapper.ConfigureMapping<StartSaga>(m => m.SomeId).ToSaga(d => d.Id);
                }

                public Task Handle(StartSaga message, IMessageHandlerContext context)
                {
                    testContext.SagaId = Data.Id;

                    return Task.FromResult(0);
                }

                public class CustomizedCollectionSagaData : ContainSagaData
                {
                }
            }
        }

        public class StartSaga : IMessage
        {
            public Guid SomeId { get; set; }
        }
    }
}