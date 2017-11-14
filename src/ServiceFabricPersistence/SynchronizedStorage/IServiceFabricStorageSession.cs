namespace NServiceBus.Persistence.ServiceFabric
{
    using Microsoft.ServiceFabric.Data;

    /// <summary>
    /// Provides access to the state manager and the current transaction.
    /// </summary>
    public interface IServiceFabricStorageSession
    {
        /// <summary>
        /// The reliable state manager.
        /// </summary>
        IReliableStateManager StateManager { get; }

        /// <summary>
        /// The current transaction.
        /// </summary>
        ITransaction Transaction { get; }
    }
}