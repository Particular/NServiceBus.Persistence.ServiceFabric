namespace NServiceBus.Persistence.ServiceFabric.SagaPersister
{
    using System.Threading.Tasks;
    using Features;
    using Microsoft.ServiceFabric.Data;

    /// <summary>
    /// Used to configure in memory saga persistence.
    /// </summary>
    public class ServiceFabricSagaPersistence : Feature
    {
        internal ServiceFabricSagaPersistence()
        {
            DependsOn<Sagas>();
            Defaults(s => s.EnableFeature(typeof(SynchronizedStorageFeature)));
        }

        /// <summary>
        /// See <see cref="Feature.Setup" />.
        /// </summary>
        protected override void Setup(FeatureConfigurationContext context)
        {
            var persister = new ServiceFabricSagaPersister();

            context.RegisterStartupTask(b => new RegisterDictionaries(context.Settings.StateManager(), persister));

            context.Container.RegisterSingleton(persister);
        }

        internal class RegisterDictionaries : FeatureStartupTask
        {
            public RegisterDictionaries(IReliableStateManager stateManager, ServiceFabricSagaPersister persister)
            {
                this.persister = persister;
                this.stateManager = stateManager;
            }

            protected override async Task OnStart(IMessageSession session)
            {
                var sagas = await stateManager.Sagas().ConfigureAwait(false);
                persister.Sagas = sagas;

                var correlations = await stateManager.Correlations().ConfigureAwait(false);
                persister.Correlations = correlations;
            }

            protected override Task OnStop(IMessageSession session)
            {
                return TaskEx.CompletedTask;
            }

            IReliableStateManager stateManager;
            ServiceFabricSagaPersister persister;
        }
    }
}