namespace NServiceBus.Persistence.ServiceFabric
{
    using Features;
    using Sagas;

    class SagaPersistenceFeature : Feature
    {
        internal SagaPersistenceFeature()
        {
            DependsOn<Sagas>();
            Defaults(s => s.Set<ISagaIdGenerator>(idGenerator));
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

            context.Container.RegisterSingleton<ISagaPersister>(persister);

            idGenerator.Initialize(infoCache);
            infoCache.Initialize(settings.Get<SagaMetadataCollection>());
        }

        SagaIdGenerator idGenerator = new SagaIdGenerator();
    }
}