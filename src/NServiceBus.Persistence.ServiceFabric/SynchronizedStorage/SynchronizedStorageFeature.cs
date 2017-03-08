namespace NServiceBus.Persistence.ServiceFabric
{
    using Features;

    class SynchronizedStorageFeature : Feature
    {
        protected override void Setup(FeatureConfigurationContext context)
        {
            //context.Container.ConfigureComponent<SynchronizedStorage>(builder => new SynchronizedStorage(context.Settings.Get("StateManager")), DependencyLifecycle.SingleInstance);
            context.Container.ConfigureComponent<SynchronizedStorage>(DependencyLifecycle.SingleInstance);
            context.Container.ConfigureComponent<SynchronizedStorageAdapter>(DependencyLifecycle.SingleInstance);
        }
    }
}