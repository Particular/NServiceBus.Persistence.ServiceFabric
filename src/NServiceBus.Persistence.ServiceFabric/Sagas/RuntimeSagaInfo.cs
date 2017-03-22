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

        public readonly ServiceFabricSagaAttribute SagaAttribute;
        readonly Version CurrentVersion;

        public RuntimeSagaInfo(Type sagaDataType,
            Type sagaType,
            JsonSerializer jsonSerializer,
            Func<TextReader, JsonReader> readerCreator,
            Func<TextWriter, JsonWriter> writerCreator)
        {
            this.sagaDataType = sagaDataType;
            this.jsonSerializer = jsonSerializer;
            this.readerCreator = readerCreator;
            this.writerCreator = writerCreator;

            var userProvidedAttribute = sagaType.GetCustomAttribute<ServiceFabricSagaAttribute>(false);

            SagaAttribute = new ServiceFabricSagaAttribute
            {
                CollectionName = userProvidedAttribute?.CollectionName ?? sagaDataType.Name,
                SagaDataName = userProvidedAttribute?.SagaDataName ?? sagaDataType.Name
            };

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
            using (var reader = new StringReader(entry.Data))
            using (var jsonReader = readerCreator(reader))
            {
                return jsonSerializer.Deserialize<TSagaData>(jsonReader);
            }
        }
    }
}