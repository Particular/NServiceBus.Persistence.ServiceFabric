namespace NServiceBus.Persistence.ServiceFabric
{
    using Features;
    using Microsoft.Extensions.DependencyInjection;
    using Settings;

    class SynchronizedStorageFeature : Feature
    {
        public SynchronizedStorageFeature()
        {
            DependsOn<SynchronizedStorage>();
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            context.Services.AddScoped<ICompletableSynchronizedStorageSession>(provider =>
            {
                var settings = provider.GetRequiredService<IReadOnlySettings>();
                var stateManager = settings.StateManager();
                var transactionTimeout = settings.TransactionTimeout();
                return new ServiceFabricStorageSession(stateManager, transactionTimeout);
            });
        }
    }
}