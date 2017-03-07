namespace NServiceBus.Persistence.ServiceFabric
{
    using Features;

    class ServiceFabricTransactionalStorageFeature : Feature
    {
        /// <summary>
        /// Called when the features is activated.
        /// </summary>
        protected override void Setup(FeatureConfigurationContext context)
        {
            //context.Container.ConfigureComponent<ServiceFabricSynchronizedStorage>(builder => new ServiceFabricSynchronizedStorage(context.Settings.Get("StateManager")), DependencyLifecycle.SingleInstance);
            context.Container.ConfigureComponent<ServiceFabricSynchronizedStorage>(DependencyLifecycle.SingleInstance);
            context.Container.ConfigureComponent<ServiceFabricTransactionalSynchronizedStorageAdapter>(DependencyLifecycle.SingleInstance);
        }
    }
}