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
            context.RegisterStartupTask(new RegisterDictionaries(context.Settings.StateManager()));

            context.Container.ConfigureComponent<ServiceFabricSagaPersister>(DependencyLifecycle.SingleInstance);
        }

        internal class RegisterDictionaries : FeatureStartupTask
        {
            public RegisterDictionaries(IReliableStateManager stateManager)
            {
                this.stateManager = stateManager;
            }

            protected override Task OnStart(IMessageSession session)
            {
                return stateManager.RegisterSagaStorage();
            }

            protected override Task OnStop(IMessageSession session)
            {
                return TaskEx.CompletedTask;
            }

            IReliableStateManager stateManager;
        }
    }
}