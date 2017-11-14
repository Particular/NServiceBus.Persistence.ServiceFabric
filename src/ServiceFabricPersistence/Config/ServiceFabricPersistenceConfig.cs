namespace NServiceBus
{
    using System;
    using Configuration.AdvancedExtensibility;
    using Microsoft.ServiceFabric.Data;
    using Persistence.ServiceFabric;
    using Settings;

    /// <summary>
    /// Configuration extensions for Service Fabric Persistence
    /// </summary>
    public static partial class ServiceFabricPersistenceConfig
    {
        /// <summary>
        /// Provides the state manager to the persistence.
        /// </summary>
        /// <param name="configuration">The persistence extension.</param>
        /// <param name="stateManager">The state manager to be used.</param>
        public static void StateManager(this PersistenceExtensions<ServiceFabricPersistence> configuration, IReliableStateManager stateManager)
        {
            Guard.AgainstNull(nameof(configuration), configuration);
            Guard.AgainstNull(nameof(stateManager), stateManager);
            configuration.GetSettings().Set("ServiceFabricPersistence.StateManager", stateManager);
        }

        internal static IReliableStateManager StateManager(this ReadOnlySettings settings)
        {
            if (settings.TryGet("ServiceFabricPersistence.StateManager", out IReliableStateManager value))
            {
                return value;
            }
            throw new Exception("StateManager must be defined.");
        }
    }
}