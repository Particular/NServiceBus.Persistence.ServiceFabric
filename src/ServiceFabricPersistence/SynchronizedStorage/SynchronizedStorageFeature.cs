namespace NServiceBus.Persistence.ServiceFabric
{
    using Features;
    using Microsoft.Extensions.DependencyInjection;

    class SynchronizedStorageFeature : Feature
    {
        protected override void Setup(FeatureConfigurationContext context)
        {
            var stateManager = context.Settings.StateManager();
            var transactionTimeout = context.Settings.TransactionTimeout();

            context.Services.AddSingleton<ISynchronizedStorage>(new SynchronizedStorage(stateManager, transactionTimeout));
            context.Services.AddSingleton<ISynchronizedStorageAdapter>(new SynchronizedStorageAdapter());
        }
    }
}