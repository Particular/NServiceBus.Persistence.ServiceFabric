namespace NServiceBus.Persistence.ServiceFabric
{
    using Gateway;
    using Outbox;
    using Persistence;
    using SagaPersister;
    using SubscriptionStorage;
    using TimeoutPersister;
    using SettingsExtensions = Features.SettingsExtensions;

    /// <summary>
    /// Used to enable InMemory persistence.
    /// </summary>
    public class ServiceFabricPersistence : PersistenceDefinition
    {
        internal ServiceFabricPersistence()
        {
            Supports<StorageType.Sagas>(s => SettingsExtensions.EnableFeatureByDefault<ServiceFabricSagaPersistence>(s));
            Supports<StorageType.Timeouts>(s => SettingsExtensions.EnableFeatureByDefault<ServiceFabricTimeoutPersistence>(s));
            Supports<StorageType.Subscriptions>(s => SettingsExtensions.EnableFeatureByDefault<ServiceFabricSubscriptionPersistence>(s));
            Supports<StorageType.Outbox>(s => SettingsExtensions.EnableFeatureByDefault<ServiceFabricOutboxPersistence>(s));
            Supports<StorageType.GatewayDeduplication>(s => SettingsExtensions.EnableFeatureByDefault<ServiceFabricGatewayPersistence>(s));
        }
    }
}