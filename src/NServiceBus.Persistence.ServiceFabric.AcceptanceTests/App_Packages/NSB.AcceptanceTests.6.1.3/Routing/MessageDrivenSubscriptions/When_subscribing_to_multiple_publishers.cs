namespace NServiceBus.AcceptanceTests.Routing.MessageDrivenSubscriptions
{
    using AcceptanceTesting;
    using EndpointTemplates;
    using Features;

    public class When_subscribing_to_multiple_publishers : NServiceBusAcceptanceTest
    {
        class Context : ScenarioContext
        {
            public bool SubscribedToPublisher1 { get; set; }
            public bool SubscribedToPublisher2 { get; set; }
        }

        class Subscriber : EndpointConfigurationBuilder
        {
            public Subscriber()
            {
                EndpointSetup<DefaultServer>(c =>
                {
                    c.DisableFeature<AutoSubscribe>();
                }, metadata =>
                {
                    metadata.RegisterPublisherFor<SomeEvent>("Publisher1");
                    metadata.RegisterPublisherFor<SomeEvent>("Publisher2");
                });
            }
        }

        class Publisher : EndpointConfigurationBuilder
        {
            public Publisher()
            {
                EndpointSetup<DefaultServer>();
            }
        }

        public class SomeEvent : IEvent
        {
        }
    }
}
