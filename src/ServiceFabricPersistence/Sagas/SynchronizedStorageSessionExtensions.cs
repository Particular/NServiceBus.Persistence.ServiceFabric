namespace NServiceBus.Persistence.ServiceFabric
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Data.Collections;

    static class SynchronizedStorageSessionExtensions
    {
        public static Task<IReliableDictionary<Guid, SagaEntry>> Sagas(this StorageSession session, string collectionName)
        {
            return session.StateManager.GetOrAddAsync<IReliableDictionary<Guid, SagaEntry>>(collectionName, session.TransactionTimeout);
        }
    }
}