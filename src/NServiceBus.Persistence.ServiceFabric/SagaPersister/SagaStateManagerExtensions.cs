﻿namespace NServiceBus.Persistence.ServiceFabric.SagaPersister
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;

    static class SagaStateManagerExtensions
    {
        public static async Task RegisterSagaStorage(this IReliableStateManager stateManager)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                await stateManager.Sagas(tx).ConfigureAwait(false);
                await stateManager.Correlations(tx).ConfigureAwait(false);

                await tx.CommitAsync().ConfigureAwait(false);
            }
        }

        public static Task<IReliableDictionary<Guid, SagaEntry>> Sagas(this IReliableStateManager stateManager, ITransaction transaction)
        {
            return stateManager.GetOrAddAsync<IReliableDictionary<Guid, SagaEntry>>(transaction, "sagas");
        }

        public static Task<IReliableDictionary<CorrelationPropertyEntry, Guid>> Correlations(this IReliableStateManager stateManager, ITransaction transaction)
        {
            return stateManager.GetOrAddAsync<IReliableDictionary<CorrelationPropertyEntry, Guid>>(transaction, "bycorrelationid");
        }
    }
}