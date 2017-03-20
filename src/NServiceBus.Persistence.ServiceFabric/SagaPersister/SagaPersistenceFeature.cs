namespace NServiceBus.Persistence.ServiceFabric
{
    using System.Threading.Tasks;
    using Features;
    using Microsoft.ServiceFabric.Data;
    using Sagas;

    class SagaPersistenceFeature : Feature
    {
        internal SagaPersistenceFeature()
        {
            DependsOn<Sagas>();
            Defaults(s => s.Set<ISagaIdGenerator>(new SagaIdGenerator()));
            Defaults(s => s.EnableFeature(typeof(SynchronizedStorageFeature)));
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            var settings = context.Settings;
            var jsonSerializerSettings = SagaSettings.GetJsonSerializerSettings(settings);
            var readerCreator = SagaSettings.GetReaderCreator(settings);
            var writerCreator = SagaSettings.GetWriterCreator(settings);
            var infoCache = new SagaInfoCache(jsonSerializerSettings, readerCreator, writerCreator);
            var persister = new SagaPersister(infoCache);

            context.RegisterStartupTask(b => new RegisterDictionaries(context.Settings.StateManager(), persister));

            context.Container.RegisterSingleton<ISagaPersister>(persister);
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