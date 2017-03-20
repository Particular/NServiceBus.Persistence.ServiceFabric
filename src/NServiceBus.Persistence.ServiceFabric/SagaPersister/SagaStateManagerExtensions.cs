namespace NServiceBus.Persistence.ServiceFabric
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;

    static class SagaStateManagerExtensions
    {
        public static async Task RegisterSagaStorage(this IReliableStateManager stateManager, SagaPersister persister)
        {
            persister.Sagas = await stateManager.Sagas().ConfigureAwait(false);
        }

        public static Task<IReliableDictionary<Guid, SagaEntry>> Sagas(this IReliableStateManager stateManager)
        {
            return stateManager.GetOrAddAsync<IReliableDictionary<Guid, SagaEntry>>("sagas");
        }
    }
}