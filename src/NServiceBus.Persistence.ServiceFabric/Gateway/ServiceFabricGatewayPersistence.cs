namespace NServiceBus.Persistence.ServiceFabric.Gateway
{
    using Features;

    /// <summary>
    /// In-memory Gateway.
    /// </summary>
    public class ServiceFabricGatewayPersistence : Feature
    {
        internal ServiceFabricGatewayPersistence()
        {
            DependsOn("NServiceBus.Features.Gateway");
        }

        /// <summary>
        /// See <see cref="Feature.Setup" />.
        /// </summary>
        protected override void Setup(FeatureConfigurationContext context)
        {
            context.Container.ConfigureComponent<ServiceFabricGatewayDeduplication>(DependencyLifecycle.SingleInstance);
        }
    }
}