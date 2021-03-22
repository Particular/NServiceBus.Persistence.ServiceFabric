namespace NServiceBus.Persistence.ServiceFabric
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Data.Collections;

    static class SynchronizedStorageSessionExtensions
    {
#pragma warning disable IDE0060 // Remove unused parameter
        public static Task<IReliableDictionary<Guid, SagaEntry>> Sagas(this StorageSession session, string collectionName, CancellationToken cancellationToken = default)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            return session.StateManager.GetOrAddAsync<IReliableDictionary<Guid, SagaEntry>>(collectionName, session.TransactionTimeout);
        }
    }
}