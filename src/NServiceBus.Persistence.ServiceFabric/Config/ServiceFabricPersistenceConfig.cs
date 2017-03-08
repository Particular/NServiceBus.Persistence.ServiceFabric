namespace NServiceBus
{
    using System;
    using Configuration.AdvanceExtensibility;
    using Microsoft.ServiceFabric.Data;
    using Persistence.ServiceFabric;
    using Settings;

    /// <summary>
    /// Configuration extensions for Service Fabric Persistence
    /// </summary>
    public static class ServiceFabricPersistenceConfig
    {
        /// <summary>
        /// Provides the state manager to the persistence.
        /// </summary>
        /// <param name="configuration">The persistence extension.</param>
        /// <param name="stateManager">The state manager to be used.</param>
        public static void StateManager(this PersistenceExtensions<ServiceFabricPersistence> configuration, IReliableStateManager stateManager)
        {
            configuration.GetSettings().Set("ServiceFabricPersistence.StateManager", stateManager);
        }

        internal static IReliableStateManager StateManager(this ReadOnlySettings settings)
        {
            IReliableStateManager value;
            if (settings.TryGet("ServiceFabricPersistence.StateManager", out value))
            {
                return value;
            }
            throw new Exception("StateManager must be defined.");
        }
    }
}