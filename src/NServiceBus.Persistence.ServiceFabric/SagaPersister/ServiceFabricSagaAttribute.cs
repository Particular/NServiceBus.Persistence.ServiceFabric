namespace NServiceBus.Persistence.ServiceFabric
{
    using System;

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class ServiceFabricSagaAttribute : Attribute
    {
        public string SagaDataName;
        public string CollectionName;
    }
}