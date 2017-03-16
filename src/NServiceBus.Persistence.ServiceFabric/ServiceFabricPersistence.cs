namespace NServiceBus.Persistence.ServiceFabric
{
    using Persistence;
    using SettingsExtensions = Features.SettingsExtensions;

    /// <summary>
    /// Used to enable Service Fabric persistence.
    /// </summary>
    public class ServiceFabricPersistence : PersistenceDefinition
    {
        internal ServiceFabricPersistence()
        {
            Supports<StorageType.Sagas>(s => SettingsExtensions.EnableFeatureByDefault<SagaPersistenceFeature>(s));
            Supports<StorageType.Outbox>(s => SettingsExtensions.EnableFeatureByDefault<OutboxPersistenceFeature>(s));
        }
    }
}