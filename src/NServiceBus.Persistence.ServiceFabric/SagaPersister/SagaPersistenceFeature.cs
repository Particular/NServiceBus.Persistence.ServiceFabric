namespace NServiceBus.Persistence.ServiceFabric.SagaPersister
{
    using System.Threading.Tasks;
    using Features;
    using Microsoft.ServiceFabric.Data;

    class SagaPersistenceFeature : Feature
    {
        internal SagaPersistenceFeature()
        {
            DependsOn<Sagas>();
            Defaults(s => s.EnableFeature(typeof(SynchronizedStorageFeature)));
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            var persister = new SagaPersister();

            context.RegisterStartupTask(b => new RegisterDictionaries(context.Settings.StateManager(), persister));

            context.Container.RegisterSingleton(persister);
        }

        class RegisterDictionaries : FeatureStartupTask
        {
            public RegisterDictionaries(IReliableStateManager stateManager, SagaPersister persister)
            {
                this.persister = persister;
                this.stateManager = stateManager;
            }

            protected override Task OnStart(IMessageSession session)
            {
                return stateManager.RegisterSagaStorage(persister);
            }

            protected override Task OnStop(IMessageSession session)
            {
                return TaskEx.CompletedTask;
            }

            IReliableStateManager stateManager;
            SagaPersister persister;
        }
    }
}