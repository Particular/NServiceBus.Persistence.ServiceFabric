namespace NServiceBus.Persistence.ServiceFabric.SagaPersister
{
    using Newtonsoft.Json;

    static class SagaEntryExtensions
    {
        public static TSagaData ToSagaData<TSagaData>(this SagaEntry entry)
        {
            return JsonConvert.DeserializeObject<TSagaData>(entry.Data);
        }

        public static string FromSagaData(this IContainSagaData data)
        {
            return JsonConvert.SerializeObject(data);
        }

        public static CorrelationPropertyEntry CopyCorrelationProperty(this SagaEntry entry)
        {
            var correlationProperty = entry.CorrelationProperty;
            return new CorrelationPropertyEntry
            {
                ExtensionData = correlationProperty.ExtensionData,
                Name = correlationProperty.Name,
                SagaDataType = correlationProperty.SagaDataType,
                Value = correlationProperty.Value,
                Type = correlationProperty.Type
            };
        }
    }
}