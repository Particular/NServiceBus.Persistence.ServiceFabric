namespace NServiceBus.Persistence.ServiceFabric
{
    using Newtonsoft.Json;
    using Settings;

    public partial class SagaSettings
    {
        public void JsonSettings(JsonSerializerSettings jsonSerializerSettings)
        {
            settings.Set("ServiceFabricPersistence.Saga.JsonSerializerSettings", jsonSerializerSettings);
        }

        internal static JsonSerializerSettings GetJsonSerializerSettings(ReadOnlySettings settings)
        {
            return settings.GetOrDefault<JsonSerializerSettings>("ServiceFabricPersistence.Saga.JsonSerializerSettings");
        }
    }
}