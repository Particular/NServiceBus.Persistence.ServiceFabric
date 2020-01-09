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
        public SagaPersister() : this(new SagaInfoCache())
        {
        }

        public SagaPersister(SagaInfoCache sagaInfoCache)
        {
            this.sagaInfoCache = sagaInfoCache;
        }

        public async Task Complete(IContainSagaData sagaData, SynchronizedStorageSession session, ContextBag context)
        {
            var storageSession = (StorageSession)session;

            var entry = GetEntry(context, sagaData.Id);

            var sagaInfo = sagaInfoCache.GetInfo(sagaData.GetType());
            var sagas = await storageSession.Sagas(sagaInfo.SagaAttribute.CollectionName).ConfigureAwait(false);

            var conditionalValue = await sagas.TryRemoveAsync(storageSession.Transaction, sagaData.Id).ConfigureAwait(false);
            if (conditionalValue.HasValue && conditionalValue.Value.Data != entry.Data)
            {
                throw new Exception("Saga can't be completed as it was updated by another process.");
            }
        }

        public async Task<TSagaData> Get<TSagaData>(Guid sagaId, SynchronizedStorageSession session, ContextBag context)
            where TSagaData : class, IContainSagaData
        {
            var storageSession = (StorageSession)session;

            var sagaInfo = sagaInfoCache.GetInfo(typeof(TSagaData));
            var sagas = await storageSession.Sagas(sagaInfo.SagaAttribute.CollectionName).ConfigureAwait(false);

            var conditionalValue = await sagas.TryGetValueAsync(storageSession.Transaction, sagaId, LockMode.Update).ConfigureAwait(false);
            if (conditionalValue.HasValue)
            {
                SetEntry(context, sagaId, conditionalValue.Value);

                return sagaInfo.FromSagaEntry<TSagaData>(conditionalValue.Value);
            }

            return default;
        }

        public Task<TSagaData> Get<TSagaData>(string propertyName, object propertyValue, SynchronizedStorageSession session, ContextBag context)
            where TSagaData : class, IContainSagaData
        {
            var sagaInfo = sagaInfoCache.GetInfo(typeof(TSagaData));
            var sagaId = SagaIdGenerator.Generate(sagaInfo, propertyName, propertyValue);

            return Get<TSagaData>(sagaId, session, context);
        }

        public async Task Save(IContainSagaData sagaData, SagaCorrelationProperty correlationProperty, SynchronizedStorageSession session, ContextBag context)
        {
            var storageSession = (StorageSession)session;
            var sagaInfo = sagaInfoCache.GetInfo(sagaData.GetType());
            var sagas = await storageSession.Sagas(sagaInfo.SagaAttribute.CollectionName).ConfigureAwait(false);

            var entry = sagaInfo.ToSagaEntry(sagaData);

            if (!await sagas.TryAddAsync(storageSession.Transaction, sagaData.Id, entry).ConfigureAwait(false))
            {
                if (correlationProperty == SagaCorrelationProperty.None)
                {
                    throw new Exception("A saga with this identifier already exists. This should never happened as saga identifier are meant to be unique.");
                }

                throw new Exception($"The saga with the correlation id 'Name: {correlationProperty.Name} Value: {correlationProperty.Value}' already exists.");
            }
        }

        public async Task Update(IContainSagaData sagaData, SynchronizedStorageSession session, ContextBag context)
        {
            var storageSession = (StorageSession)session;

            var loadedEntry = GetEntry(context, sagaData.Id);

            var sagaInfo = sagaInfoCache.GetInfo(sagaData.GetType());
            var sagas = await storageSession.Sagas(sagaInfo.SagaAttribute.CollectionName).ConfigureAwait(false);

            var newEntry = sagaInfo.ToSagaEntry(sagaData);

            if (!await sagas.TryUpdateAsync(storageSession.Transaction, sagaData.Id, newEntry, loadedEntry).ConfigureAwait(false))
            {
                throw new Exception($"{nameof(SagaPersister)} concurrency violation: saga entity Id[{sagaData.Id}] already saved.");
            }
        }

        static void SetEntry(ContextBag context, Guid sagaId, SagaEntry value)
        {
            if (context.TryGet(ContextKey, out Dictionary<Guid, SagaEntry> entries) == false)
            {
                entries = new Dictionary<Guid, SagaEntry>();
                context.Set(ContextKey, entries);
            }

            entries[sagaId] = value;
        }

        static SagaEntry GetEntry(ReadOnlyContextBag context, Guid sagaDataId)
        {
            if (context.TryGet(ContextKey, out Dictionary<Guid, SagaEntry> entries))
            {
                if (entries.TryGetValue(sagaDataId, out var entry))
                {
                    return entry;
                }
            }

            throw new Exception("The saga should be retrieved with Get method before it's updated");
        }

        SagaInfoCache sagaInfoCache;

        const string ContextKey = "NServiceBus.Persistence.ServiceFabric.Sagas";
    }
}