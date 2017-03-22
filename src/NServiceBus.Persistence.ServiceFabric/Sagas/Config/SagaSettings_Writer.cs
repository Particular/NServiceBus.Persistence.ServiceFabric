namespace NServiceBus.Persistence.ServiceFabric
{
    using System;
    using System.IO;
    using System.Text;
    using Newtonsoft.Json;
    using Settings;

    public partial class SagaSettings
    {

        public void WriterCreator(Func<StringBuilder, JsonWriter> writerCreator)
        {
            settings.Set("ServiceFabricPersistence.Saga.WriterCreator", writerCreator);
        }

        internal static Func<TextWriter, JsonWriter> GetWriterCreator(ReadOnlySettings settings)
        {
            return settings.GetOrDefault<Func<TextWriter, JsonWriter>>("ServiceFabricPersistence.Saga.WriterCreator");
        }

    }
}