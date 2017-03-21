namespace NServiceBus.Persistence.ServiceFabric
{
    using System;
    using Extensibility;
    using Sagas;

    static class ContextBagExtentions
    {
        public static Type GetSagaType(this ContextBag context)
        {
            var activeSagaInstance = context.Get<ActiveSagaInstance>();
            if (activeSagaInstance != null)
            {
                return activeSagaInstance.Instance.GetType();
            }
            throw new Exception($"Expected to find an instance of {nameof(ActiveSagaInstance)} in the {nameof(ContextBag)}.");
        }
    }
}