namespace NServiceBus.Persistence.ServiceFabric.SagaPersister
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Extensibility;
    using Newtonsoft.Json;
    using Sagas;

    class ServiceFabricSagaPersister : ISagaPersister
    {
        public async Task Complete(IContainSagaData sagaData, SynchronizedStorageSession session, ContextBag context)
        {
            var storageSession = (StorageSession)session;
            var tx = storageSession.Transaction;

            var sagasDictionary = await storageSession.StateManager.Sagas(tx).ConfigureAwait(false);

            var entry = GetEntry(context, sagaData.Id);

            var conditionalValue = await sagasDictionary.TryRemoveAsync(tx, sagaData.Id).ConfigureAwait(false);
            if (conditionalValue.HasValue && conditionalValue.Value.Data != entry.Data)
            {
                throw new Exception("Saga can't be completed as it was updated by another process.");
            }

            // saga removed
            // clean the index
            if (Equals(entry.CorrelationProperty, NoCorrelationId) == false)
            {
                var byCorrelationId = await storageSession.StateManager.Correlations(tx).ConfigureAwait(false);

                await byCorrelationId.TryRemoveAsync(tx, entry.CorrelationProperty).ConfigureAwait(false);
            }
        }

        public async Task<TSagaData> Get<TSagaData>(Guid sagaId, SynchronizedStorageSession session, ContextBag context)
            where TSagaData : IContainSagaData
        {
            var storageSession = (StorageSession) session;
            var tx = storageSession.Transaction;

            var sagasDictionary = await storageSession.StateManager.Sagas(tx).ConfigureAwait(false);
            var conditionalValue = await sagasDictionary.TryGetValueAsync(tx, sagaId).ConfigureAwait(false);
            if (conditionalValue.HasValue)
            {
                SetEntry(context, sagaId, conditionalValue.Value);
                return conditionalValue.Value.ToSagaData<TSagaData>();
            }
            return default(TSagaData);
        }

        public async Task<TSagaData> Get<TSagaData>(string propertyName, object propertyValue, SynchronizedStorageSession session, ContextBag context) where TSagaData : IContainSagaData
        {
            var storageSession = (StorageSession) session;
            var tx = storageSession.Transaction;

            var serializedPropertyValue = JsonConvert.SerializeObject(propertyValue);
            var key = new CorrelationPropertyEntry
            {
                SagaDataType = typeof(TSagaData).FullName,
                Name = propertyName,
                Value = serializedPropertyValue,
                Type = propertyValue.GetType().FullName
            };
            var byCorrelationId = await storageSession.StateManager.Correlations(tx).ConfigureAwait(false);
            var conditionalValue = await byCorrelationId.TryGetValueAsync(tx, key).ConfigureAwait(false);
            if (conditionalValue.HasValue)
            {
                return await Get<TSagaData>(conditionalValue.Value, session, context).ConfigureAwait(false);
            }
            return default(TSagaData);
        }

        public async Task Save(IContainSagaData sagaData, SagaCorrelationProperty correlationProperty, SynchronizedStorageSession session, ContextBag context)
        {
            var storageSession = (StorageSession)session;
            var tx = storageSession.Transaction;

            var correlationId = NoCorrelationId;
            if (correlationProperty != SagaCorrelationProperty.None)
            {
                var serializedPropertyValue = JsonConvert.SerializeObject(correlationProperty.Value);
                correlationId = new CorrelationPropertyEntry
                {
                    SagaDataType = sagaData.GetType().FullName,
                    Name = correlationProperty.Name,
                    Value = serializedPropertyValue,
                    Type = correlationProperty.Value.GetType().FullName
                };

                var byCorrelationId = await storageSession.StateManager.Correlations(tx).ConfigureAwait(false);

                if (!await byCorrelationId.TryAddAsync(tx, correlationId, sagaData.Id).ConfigureAwait(false))
                {
                    throw new Exception($"The saga with the correlation id 'Name: {correlationProperty.Name} Value: {correlationProperty.Value}' already exists");
                }
            }

            var entry = new SagaEntry
            {
                CorrelationProperty = correlationId,
                Data = sagaData.FromSagaData(),
            };
            var sagasDictionary = await storageSession.StateManager.Sagas(tx).ConfigureAwait(false);
            if (!await sagasDictionary.TryAddAsync(tx, sagaData.Id, entry).ConfigureAwait(false))
            {
                throw new Exception("A saga with this identifier already exists. This should never happened as saga identifier are meant to be unique.");
            }
        }

        public async Task Update(IContainSagaData sagaData, SynchronizedStorageSession session, ContextBag context)
        {
            var storageSession = (StorageSession)session;
            var tx = storageSession.Transaction;

            var sagasDictionary = await storageSession.StateManager.Sagas(tx).ConfigureAwait(false);

            var loadedEntry = GetEntry(context, sagaData.Id);

            var newEntry = new SagaEntry
            {
                CorrelationProperty = loadedEntry.CopyCorrelationProperty(),
                Data = sagaData.FromSagaData(),
            };

            if (!await sagasDictionary.TryUpdateAsync(tx, sagaData.Id, newEntry, loadedEntry).ConfigureAwait(false))
            {
                throw new Exception($"InMemorySagaPersister concurrency violation: saga entity Id[{sagaData.Id}] already saved.");
            }
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

        static readonly CorrelationPropertyEntry NoCorrelationId = new CorrelationPropertyEntry
        {
            SagaDataType = typeof(object).FullName,
            Name = null,
            Value = null,
            Type = typeof(object).FullName
        };
    }
}