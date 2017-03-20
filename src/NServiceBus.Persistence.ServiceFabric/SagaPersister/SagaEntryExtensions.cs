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
    }
}