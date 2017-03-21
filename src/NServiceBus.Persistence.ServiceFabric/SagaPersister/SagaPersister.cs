namespace NServiceBus.Persistence.ServiceFabric
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Extensibility;
    using Microsoft.ServiceFabric.Data.Collections;
    using Sagas;

    class SagaPersister : ISagaPersister
    {
        public IReliableDictionary<Guid, SagaEntry> Sagas { get; set; }

        public Task Complete(IContainSagaData sagaData, SynchronizedStorageSession session, ContextBag context)
        {
            var storageSession = (StorageSession)session;

            var entry = GetEntry(context, sagaData.Id);

            return storageSession.Add(async tx =>
            {
                var conditionalValue = await Sagas.TryRemoveAsync(tx, sagaData.Id).ConfigureAwait(false);
                if (conditionalValue.HasValue && conditionalValue.Value.Data != entry.Data)
                {
                    throw new Exception("Saga can't be completed as it was updated by another process.");
                }
            });
        }

        public async Task<TSagaData> Get<TSagaData>(Guid sagaId, SynchronizedStorageSession session, ContextBag context)
            where TSagaData : IContainSagaData
        {
            var storageSession = (StorageSession) session;
            using (var tx = storageSession.StateManager.CreateTransaction())
            {
                var conditionalValue = await Sagas.TryGetValueAsync(tx, sagaId).ConfigureAwait(false);
                if (conditionalValue.HasValue)
                {
                    SetEntry(context, sagaId, conditionalValue.Value);
                    return conditionalValue.Value.ToSagaData<TSagaData>();
                }
                return default(TSagaData);
            }
        }

        public Task<TSagaData> Get<TSagaData>(string propertyName, object propertyValue, SynchronizedStorageSession session, ContextBag context) where TSagaData : IContainSagaData
        {
            var sagaId = SagaIdGenerator.Generate(typeof(TSagaData), propertyName, propertyValue);

            return Get<TSagaData>(sagaId, session, context);
        }

        public Task Save(IContainSagaData sagaData, SagaCorrelationProperty correlationProperty, SynchronizedStorageSession session, ContextBag context)
        {
            var storageSession = (StorageSession)session;

            var entry = new SagaEntry(sagaData.FromSagaData());

            return storageSession.Add(async tx =>
            {
                if (!await Sagas.TryAddAsync(tx, sagaData.Id, entry).ConfigureAwait(false))
                {
                    if (correlationProperty == SagaCorrelationProperty.None)
                    {
                        throw new Exception("A saga with this identifier already exists. This should never happened as saga identifier are meant to be unique.");
                    }
                    throw new Exception($"The saga with the correlation id 'Name: {correlationProperty.Name} Value: {correlationProperty.Value}' already exists.");
                }
            });
        }

        public Task Update(IContainSagaData sagaData, SynchronizedStorageSession session, ContextBag context)
        {
            var storageSession = (StorageSession)session;

            var loadedEntry = GetEntry(context, sagaData.Id);

            var newEntry = new SagaEntry(sagaData.FromSagaData());

            return storageSession.Add(async tx =>
            {
                if (!await Sagas.TryUpdateAsync(tx, sagaData.Id, newEntry, loadedEntry).ConfigureAwait(false))
                {
                    throw new Exception($"{nameof(SagaPersister)} concurrency violation: saga entity Id[{sagaData.Id}] already saved.");
                }
            });
        }

        static void SetEntry(ContextBag context, Guid sagaId, SagaEntry value)
        {
            Dictionary<Guid, SagaEntry> entries;
            if (context.TryGet(ContextKey, out entries) == false)
            {
                entries = new Dictionary<Guid, SagaEntry>();
                context.Set(ContextKey, entries);
            }
            entries[sagaId] = value;
        }

        static SagaEntry GetEntry(ReadOnlyContextBag context, Guid sagaDataId)
        {
            Dictionary<Guid, SagaEntry> entries;
            if (context.TryGet(ContextKey, out entries))
            {
                SagaEntry entry;

                if (entries.TryGetValue(sagaDataId, out entry))
                {
                    return entry;
                }
            }
            throw new Exception("The saga should be retrieved with Get method before it's updated");
        }

        const string ContextKey = "NServiceBus.Persistence.ServiceFabric.Sagas";
    }
}