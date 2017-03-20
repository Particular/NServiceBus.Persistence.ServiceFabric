namespace NServiceBus.Persistence.ServiceFabric
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using Newtonsoft.Json;

    class SagaInfoCache
    {
        JsonSerializer jsonSerializer;
        Func<TextReader, JsonReader> readerCreator;
        Func<TextWriter, JsonWriter> writerCreator;
        ConcurrentDictionary<Type, RuntimeSagaInfo> serializerCache = new ConcurrentDictionary<Type, RuntimeSagaInfo>();

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

        public RuntimeSagaInfo GetInfo(Type sagaDataType, Type sagaType)
        {
            return serializerCache.GetOrAdd(sagaDataType, dataType => BuildSagaInfo(dataType, sagaType));
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