namespace NServiceBus.Persistence.ServiceFabric.TimeoutPersister
{
    using System;
    using Features;

    /// <summary>
    /// Used to configure in memory timeout persistence.
    /// </summary>
    public class ServiceFabricTimeoutPersistence : Feature
    {
        internal ServiceFabricTimeoutPersistence()
        {
            DependsOn<TimeoutManager>();
        }

        /// <summary>
        /// See <see cref="Feature.Setup" />.
        /// </summary>
        protected override void Setup(FeatureConfigurationContext context)
        {
            context.Container.ConfigureComponent(_ => new ServiceFabricTimeoutPersister(() => DateTime.UtcNow), DependencyLifecycle.SingleInstance);
        }
    }
}