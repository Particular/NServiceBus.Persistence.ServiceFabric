namespace NServiceBus.Persistence.ServiceFabric
{
    using Outbox;
    using Persistence;
    using SagaPersister;
    using SettingsExtensions = Features.SettingsExtensions;

    /// <summary>
    /// Used to enable InMemory persistence.
    /// </summary>
    public class ServiceFabricPersistence : PersistenceDefinition
    {
        internal ServiceFabricPersistence()
        {
            Supports<StorageType.Sagas>(s => SettingsExtensions.EnableFeatureByDefault<SagaPersistenceFeature>(s));
            Supports<StorageType.Outbox>(s => SettingsExtensions.EnableFeatureByDefault<ServiceFabricOutboxPersistence>(s));
        }
    }
}