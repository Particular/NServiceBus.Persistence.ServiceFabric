namespace NServiceBus.Persistence.ServiceFabric
{
    using System;
    using System.IO;
    using Newtonsoft.Json;
    using Settings;

    public partial class SagaSettings
    {
        public void ReaderCreator(Func<TextReader, JsonReader> readerCreator)
        {
            settings.Set("ServiceFabricPersistence.Saga.ReaderCreator", readerCreator);
        }

        internal static Func<TextReader, JsonReader> GetReaderCreator(IReadOnlySettings settings)
        {
            return settings.GetOrDefault<Func<TextReader, JsonReader>>("ServiceFabricPersistence.Saga.ReaderCreator");
        }
    }
}