namespace NServiceBus.Persistence.ServiceFabric
{
    using Features;
    using Microsoft.Extensions.DependencyInjection;
    using Sagas;

    class SagaPersistenceFeature : Feature
    {
        internal SagaPersistenceFeature()
        {
            Defaults(s =>
            {
                s.SetDefault<ISagaIdGenerator>(idGenerator);
                s.EnableFeatureByDefault<SynchronizedStorageFeature>();
            });

            DependsOn<Sagas>();
            DependsOn<SynchronizedStorageFeature>();
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            var settings = context.Settings;
            var jsonSerializerSettings = SagaSettings.GetJsonSerializerSettings(settings);
            var readerCreator = SagaSettings.GetReaderCreator(settings);
            var writerCreator = SagaSettings.GetWriterCreator(settings);

            var infoCache = new SagaInfoCache(jsonSerializerSettings, readerCreator, writerCreator);

            context.Services.AddSingleton<ISagaPersister>(new SagaPersister(infoCache));

            idGenerator.Initialize(infoCache);
            infoCache.Initialize(settings.Get<SagaMetadataCollection>());
        }

        SagaIdGenerator idGenerator = new SagaIdGenerator();
    }
}