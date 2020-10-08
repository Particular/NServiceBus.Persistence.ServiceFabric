namespace NServiceBus.Testing
{
    using Microsoft.ServiceFabric.Data;
    using Persistence;
    using Persistence.ServiceFabric;

    /// <summary>
    /// A fake implementation for <see cref="SynchronizedStorageSession"/> for testing purposes.
    /// </summary>
    public class TestableServiceFabricStorageSession : SynchronizedStorageSession, IServiceFabricStorageSession
    {
        /// <summary>
        /// Creates a new instance of <see cref="TestableServiceFabricStorageSession"/> using the provided <see cref="IReliableStateManager"/> and <see cref="ITransaction"/>.
        /// </summary>
        /// <param name="stateManager">The reliable state manager to use.</param>
        /// <param name="transaction">The transaction to use.</param>
        public TestableServiceFabricStorageSession(IReliableStateManager stateManager, ITransaction transaction)
        {
            StateManager = stateManager;
            Transaction = transaction;
        }

        /// <summary>
        /// The reliable state manager which is retrieved by calling <see cref="ServiceFabricPersistenceStorageSessionExtensions.ServiceFabricSession"/>.
        /// </summary>
        public IReliableStateManager StateManager { get; }

        /// <summary>
        /// The transaction which is retrieved by calling <see cref="ServiceFabricPersistenceStorageSessionExtensions.ServiceFabricSession"/>.
        /// </summary>
        public ITransaction Transaction { get; }
    }
}