namespace NServiceBus.Persistence.ServiceFabric.SagaPersister
{
    using System.Runtime.Serialization;

    [DataContract(Namespace = "NServiceBus.Persistence.ServiceFabric", Name = "SagaEntry")]
    internal sealed class SagaEntry : IExtensibleDataObject
    {
        [DataMember(Name = "CorrelationProperty", Order = 0)]
        public CorrelationPropertyEntry CorrelationProperty { get; set; }

        [DataMember(Name = "Data", Order = 1)]
        public string Data { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }
}