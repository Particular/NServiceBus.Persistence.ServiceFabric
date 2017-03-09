namespace NServiceBus.Persistence.ServiceFabric.SagaPersister
{
    using Newtonsoft.Json;

    static class SagaEntryExtensions
    {
        public static TSagaData ToSagaData<TSagaData>(this SagaEntry entry)
        {
            return JsonConvert.DeserializeObject<TSagaData>(entry.Data);
        }
    }
}