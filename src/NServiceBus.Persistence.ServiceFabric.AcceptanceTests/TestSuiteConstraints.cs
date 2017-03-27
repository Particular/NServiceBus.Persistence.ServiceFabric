namespace NServiceBus.AcceptanceTests
{
    using AcceptanceTesting.Support;

    public partial class TestSuiteConstraints
    {
        public bool SupportsDtc { get; } = false;
        public bool SupportsCrossQueueTransactions { get; } = true;
        public bool SupportsNativePubSub { get; } = true;
        public bool SupportsNativeDeferral { get; } = true;
        public bool SupportsOutbox { get; } = true;
        public IConfigureEndpointTestExecution TransportConfiguration { get; } = new ConfigureEndpointAzureServiceBusTransport();
        public IConfigureEndpointTestExecution PersistenceConfiguration { get; } = new ConfigureEndpointServiceFabricPersistence();
    }
}