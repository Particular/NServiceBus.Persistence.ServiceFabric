namespace NServiceBus.Persistence.ServiceFabric
{
    using System;

    /// <summary>
    /// Shared session extensions for ServiceFabricPersistence.
    /// </summary>
    public static class ServiceFabricPersistenceStorageSessionExtensions
    {
        /// <summary>
        /// Gets the current context ServiceFabricPersistence <see cref="IServiceFabricStorageSession"/>.
        /// </summary>
        public static IServiceFabricStorageSession ServiceFabricSession(this SynchronizedStorageSession session)
        {
            Guard.AgainstNull(nameof(session), session);
            return GetServiceFabricSession(session);
        }

        static IServiceFabricStorageSession GetServiceFabricSession(this SynchronizedStorageSession session)
        {
            if (session is IServiceFabricStorageSession storageSession)
            {
                return storageSession;
            }
            throw new Exception("The endpoint has not been configured to use Service Fabric persistence.");
        }
    }
}