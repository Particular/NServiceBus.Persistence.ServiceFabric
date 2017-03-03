namespace NServiceBus.Persistence.ServiceFabric.SagaPersister
{
    using Features;

    /// <summary>
    /// Used to configure in memory saga persistence.
    /// </summary>
    public class ServiceFabricSagaPersistence : Feature
    {
        internal ServiceFabricSagaPersistence()
        {
            DependsOn<Sagas>();
            Defaults(s => s.EnableFeature(typeof(ServiceFabricTransactionalStorageFeature)));
        }

        /// <summary>
        /// See <see cref="Feature.Setup" />.
        /// </summary>
        protected override void Setup(FeatureConfigurationContext context)
        {
            context.Container.ConfigureComponent<ServiceFabricSagaPersister>(DependencyLifecycle.SingleInstance);
        }
    }
}