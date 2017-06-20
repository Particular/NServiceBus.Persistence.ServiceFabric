namespace NServiceBus.Persistence.ServiceFabric.AcceptanceTests
{
    using System;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using Microsoft.ServiceFabric.Data.Collections;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NUnit.Framework;

    public class When_trying_to_share_outbox_transaction : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_share_it()
        {
            Requires.OutboxPersistence();

            var context = await Scenario.Define<Context>()
                .WithEndpoint<NonDtcReceivingEndpoint>(b => b.When(session => session.SendLocal(new PlaceOrder())))
                .Done(c => c.OrderAckReceived == 1)
                .Run(TimeSpan.FromSeconds(20));

            Assert.AreEqual(1, context.OrderAckReceived, "Order ack should have been received");
            Assert.AreEqual(context.TestRunId.ToString(), context.Value, "TestRunId should have been stored in dictionary");
        }

        class Context : ScenarioContext
        {
            public int OrderAckReceived { get; set; }
            public string Value { get; set; }
        }

        public class NonDtcReceivingEndpoint : EndpointConfigurationBuilder
        {
            public NonDtcReceivingEndpoint()
            {
                EndpointSetup<DefaultServer>(b =>
                {
                    b.EnableOutbox();
                });
            }

            class PlaceOrderHandler : IHandleMessages<PlaceOrder>
            {
                Context testContext;

                public PlaceOrderHandler(Context testContext)
                {
                    this.testContext = testContext;
                }

                public async Task Handle(PlaceOrder message, IMessageHandlerContext context)
                {
                    var session = context.SynchronizedStorageSession.ServiceFabricSession();

                    var stateManager = session.StateManager;
                    var handlertransaction = session.Transaction;

                    var testRunId = testContext.TestRunId.ToString(); // use the test run id as dictionary name and data

                    IReliableDictionary<string, string> dict;
                    using (var customtransaction = stateManager.CreateTransaction())
                    {
                        dict = await stateManager.GetOrAddAsync<IReliableDictionary<string, string>>(customtransaction, testRunId)
                            .ConfigureAwait(false);

                        await customtransaction.CommitAsync().ConfigureAwait(false);
                    }

                    await dict.AddAsync(handlertransaction, testRunId, testRunId).ConfigureAwait(false);

                    await context.SendLocal(new SendOrderAcknowledgement());
                }
            }

            class SendOrderAcknowledgementHandler : IHandleMessages<SendOrderAcknowledgement>
            {
                Context testContext;

                public SendOrderAcknowledgementHandler(Context testContext)
                {
                    this.testContext = testContext;
                }

                public async Task Handle(SendOrderAcknowledgement message, IMessageHandlerContext context)
                {
                    var session = context.SynchronizedStorageSession.ServiceFabricSession();
                    var stateManager = session.StateManager;
                    var handlertransaction = session.Transaction;
                    var testRunId = testContext.TestRunId.ToString();

                    var dict = await stateManager.GetOrAddAsync<IReliableDictionary<string, string>>(handlertransaction, testRunId)
                        .ConfigureAwait(false);
                    var conditionalValue = await dict.TryGetValueAsync(handlertransaction, testRunId);

                    testContext.Value = conditionalValue.Value;
                    testContext.OrderAckReceived++;
                }
            }
        }

        public class PlaceOrder : ICommand
        {
        }

        public class SendOrderAcknowledgement : IMessage
        {
        }
    }
}