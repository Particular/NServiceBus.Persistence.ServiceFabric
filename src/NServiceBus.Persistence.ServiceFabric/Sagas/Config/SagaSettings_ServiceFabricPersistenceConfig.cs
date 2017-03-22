namespace NServiceBus
{
    using Configuration.AdvanceExtensibility;
    using Persistence.ServiceFabric;

    public static partial class ServiceFabricPersistenceConfig
    {
        public static SagaSettings SagaSettings(this PersistenceExtensions<ServiceFabricPersistence> configuration)
        {
            return new SagaSettings(configuration.GetSettings());
        }
    }
}