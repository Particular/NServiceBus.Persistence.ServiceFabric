namespace NServiceBus.Persistence.ServiceFabric
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using Newtonsoft.Json;

    class RuntimeSagaInfo
    {
        // ReSharper disable once NotAccessedField.Local
        Type sagaDataType;
        JsonSerializer jsonSerializer;
        Func<TextReader, JsonReader> readerCreator;
        Func<TextWriter, JsonWriter> writerCreator;

        public ServiceFabricSagaAttribute SagaAttribute { get; private set; }

        readonly Version CurrentVersion;

        public RuntimeSagaInfo(Type sagaDataType,
            // ReSharper disable once UnusedParameter.Local
            Type sagaType,
            JsonSerializer jsonSerializer,
            Func<TextReader, JsonReader> readerCreator,
            Func<TextWriter, JsonWriter> writerCreator)
        {
            this.sagaDataType = sagaDataType;
            this.jsonSerializer = jsonSerializer;
            this.readerCreator = readerCreator;
            this.writerCreator = writerCreator;

            //TODO: make sure we support the setting only collectionName or sagaDataName
            this.SagaAttribute = sagaType.GetCustomAttribute<ServiceFabricSagaAttribute>(false) ??
                new ServiceFabricSagaAttribute {CollectionName = sagaDataType.Name, SagaDataName = sagaDataType.Name};

            CurrentVersion = sagaDataType.Assembly.GetFileVersion();
        }

        public SagaEntry ToSagaEntry(IContainSagaData sagaData)
        {
            var builder = new StringBuilder();
            using (var stringWriter = new StringWriter(builder))
            using (var writer = writerCreator(stringWriter))
            {
                jsonSerializer.Serialize(writer, sagaData);
            }
            return new SagaEntry(builder.ToString(), CurrentVersion, StaticVersions.PersistenceVersion);
        }

        public TSagaData FromSagaEntry<TSagaData>(SagaEntry entry)
            where TSagaData : IContainSagaData
        {
            using(var reader = new StringReader(entry.Data))
            using (var jsonReader = readerCreator(reader))
            {
                return jsonSerializer.Deserialize<TSagaData>(jsonReader);
            }
        }
    }
}