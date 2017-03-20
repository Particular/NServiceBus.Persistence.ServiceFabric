namespace NServiceBus.Persistence.ServiceFabric
{
    using System.Runtime.Serialization;

    [DataContract(Namespace = "NServiceBus.Persistence.ServiceFabric", Name = "SagaEntry")]
    sealed class SagaEntry : IExtensibleDataObject
    {
        public SagaEntry(string data)
        {
            Data = data;
        }

        [DataMember(Name = "Data", Order = 0)]
        public string Data { get; private set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }
}