namespace NServiceBus.Persistence.ServiceFabric
{
    using Newtonsoft.Json;

    static class SagaEntryExtensions
    {
        public static string FromSagaData(this IContainSagaData data)
        {
            return JsonConvert.SerializeObject(data);
        }
    }
}