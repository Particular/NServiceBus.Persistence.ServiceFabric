namespace NServiceBus.Persistence.ServiceFabric
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract(Namespace = "NServiceBus.Persistence.ServiceFabric", Name = "StoredOutboxMessage")]
    public sealed class StoredTransportOperation : IExtensibleDataObject
    {
        [DataMember(Name = "MessageId", Order = 0)]
        public string MessageId { get; private set; }

        [DataMember(Name = "Options", Order = 1)]
        public Dictionary<string, string> Options { get; private set; }

        [DataMember(Name = "Body", Order = 2)]
        public byte[] Body { get; private set; }

        [DataMember(Name = "Headers", Order = 3)]
        public Dictionary<string, string> Headers { get; private set; }

        public ExtensionDataObject ExtensionData { get; set; }

        public StoredTransportOperation(string messageId, Dictionary<string, string> options, byte[] body, Dictionary<string, string> headers)
        {
            MessageId = messageId;
            Options = options;
            Body = body;
            Headers = headers;
        }
    }
}