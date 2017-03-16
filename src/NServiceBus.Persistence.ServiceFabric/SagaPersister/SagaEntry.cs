namespace NServiceBus.Persistence.ServiceFabric.SagaPersister
{
    using System.Runtime.Serialization;

    [DataContract(Namespace = "NServiceBus.Persistence.ServiceFabric", Name = "SagaEntry")]
    sealed class SagaEntry : IExtensibleDataObject
    {
        public SagaEntry(CorrelationPropertyEntry correlationProperty, string data)
        {
            CorrelationProperty = correlationProperty;
            Data = data;
        }

        [DataMember(Name = "CorrelationProperty", Order = 0)]
        public CorrelationPropertyEntry CorrelationProperty { get; private set; }

        [DataMember(Name = "Data", Order = 1)]
        public string Data { get; private set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }
}