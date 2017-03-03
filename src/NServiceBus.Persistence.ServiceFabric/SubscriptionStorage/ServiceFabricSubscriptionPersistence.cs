namespace NServiceBus.Persistence.ServiceFabric.SubscriptionStorage
{
    using Features;

    /// <summary>
    /// Used to configure in memory subscription persistence.
    /// </summary>
    public class ServiceFabricSubscriptionPersistence : Feature
    {
        internal ServiceFabricSubscriptionPersistence()
        {
            DependsOn<MessageDrivenSubscriptions>();
        }

        /// <summary>
        /// See <see cref="Feature.Setup" />.
        /// </summary>
        protected override void Setup(FeatureConfigurationContext context)
        {
            context.Container.ConfigureComponent<ServiceFabricSubscriptionStorage>(DependencyLifecycle.SingleInstance);
        }
    }
}