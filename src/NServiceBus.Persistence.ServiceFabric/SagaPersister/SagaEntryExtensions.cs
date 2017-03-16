namespace NServiceBus.Persistence.ServiceFabric
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
            return new CorrelationPropertyEntry(correlationProperty.SagaDataType, correlationProperty.Name, correlationProperty.Value, correlationProperty.Type)
            {
                ExtensionData = correlationProperty.ExtensionData,
            };
        }
    }
}