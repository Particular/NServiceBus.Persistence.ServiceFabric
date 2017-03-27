namespace NServiceBus.Persistence.ServiceFabric
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Newtonsoft.Json;
    using Sagas;

    class SagaInfoCache
    {
        JsonSerializer jsonSerializer;
        Func<TextReader, JsonReader> readerCreator;
        Func<TextWriter, JsonWriter> writerCreator;
        Dictionary<Type, RuntimeSagaInfo> serializerCache = new Dictionary<Type, RuntimeSagaInfo>();

        public SagaInfoCache() : this(null, null, null)
        {
        }

        public SagaInfoCache(
            JsonSerializerSettings jsonSerializerSettings,
            Func<TextReader, JsonReader> readerCreator,
            Func<TextWriter, JsonWriter> writerCreator)
        {
            this.writerCreator = writerCreator ?? (writer => new JsonTextWriter(writer));
            this.readerCreator = readerCreator ?? (reader => new JsonTextReader(reader));
            jsonSerializer = JsonSerializer.Create(jsonSerializerSettings ?? new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                DefaultValueHandling = DefaultValueHandling.Ignore
            });
        }

        public RuntimeSagaInfo GetInfo(Type sagaDataType)
        {
            RuntimeSagaInfo sagaInfo;
            if (!serializerCache.TryGetValue(sagaDataType, out sagaInfo))
            {
                throw new Exception($"Unable to retrieve sage information for {sagaDataType}.");
            }
            return sagaInfo;
        }

        public void Initialize(SagaMetadataCollection metadataCollection)
        {
            foreach (var metadata in metadataCollection)
            {
                serializerCache[metadata.SagaEntityType] = BuildSagaInfo(metadata.SagaEntityType, metadata.SagaType);
            }
        }

        RuntimeSagaInfo BuildSagaInfo(Type sagaDataType, Type sagaType)
        {
            return new RuntimeSagaInfo(
                sagaDataType,
                sagaType,
                jsonSerializer,
                readerCreator,
                writerCreator);
        }
    }
}