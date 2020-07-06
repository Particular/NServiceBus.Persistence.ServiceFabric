namespace NServiceBus.Persistence.ServiceFabric
{
    using Extensibility;

    public partial class SagaSettings
    {
        public void UseOptimisticConcurrency()
        {
            settings.Set("NServiceBus.Persistence.ServiceFabric.UseOptimisticConcurrency", true);
        }

        internal static bool GetOptimisticConcurrency(ContextBag settings)
        {
            return settings.TryGet<bool>("NServiceBus.Persistence.ServiceFabric.UseOptimisticConcurrency", out _);
        }
    }
}