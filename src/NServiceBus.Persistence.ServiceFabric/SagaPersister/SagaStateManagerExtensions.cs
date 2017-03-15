namespace NServiceBus.Persistence.ServiceFabric.SagaPersister
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;

    static class SagaStateManagerExtensions
    {
        public static async Task RegisterSagaStorage(this IReliableStateManager stateManager, SagaPersister persister)
        {
            var sagas = await stateManager.Sagas().ConfigureAwait(false);
            persister.Sagas = sagas;

            var correlations = await stateManager.Correlations().ConfigureAwait(false);
            persister.Correlations = correlations;
        }

        public static Task<IReliableDictionary<Guid, SagaEntry>> Sagas(this IReliableStateManager stateManager)
        {
            return stateManager.GetOrAddAsync<IReliableDictionary<Guid, SagaEntry>>("sagas");
        }

        public static Task<IReliableDictionary<Guid, SagaEntry>> Sagas(this IReliableStateManager stateManager, ITransaction transaction)
        {
            return stateManager.GetOrAddAsync<IReliableDictionary<Guid, SagaEntry>>(transaction, "sagas");
        }

        public static Task<IReliableDictionary<CorrelationPropertyEntry, Guid>> Correlations(this IReliableStateManager stateManager)
        {
            return stateManager.GetOrAddAsync<IReliableDictionary<CorrelationPropertyEntry, Guid>>("bycorrelationid");
        }

        public static Task<IReliableDictionary<CorrelationPropertyEntry, Guid>> Correlations(this IReliableStateManager stateManager, ITransaction transaction)
        {
            return stateManager.GetOrAddAsync<IReliableDictionary<CorrelationPropertyEntry, Guid>>(transaction, "bycorrelationid");
        }
    }
}