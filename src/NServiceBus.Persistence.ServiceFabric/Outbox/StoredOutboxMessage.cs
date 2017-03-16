namespace NServiceBus.Persistence.ServiceFabric.Outbox
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract(Namespace = "NServiceBus.Persistence.ServiceFabric", Name = "StoredOutboxMessage")]
    sealed class StoredOutboxMessage : IExtensibleDataObject
    {
        public StoredOutboxMessage(string messageId, StoredTransportOperation[] transportOperations)
        {
            TransportOperations = transportOperations;
            Id = messageId;
            StoredAt = DateTimeOffset.UtcNow;
        }

        StoredOutboxMessage(string messageId, DateTimeOffset storedAt, bool dispatched)
        {
            TransportOperations = EmptyTransportOperations;
            Id = messageId;
            StoredAt = storedAt;
            Dispatched = dispatched;
        }

        [DataMember(Name = "Id", Order = 0)]
        public string Id { get; private set; }

        [DataMember(Name = "Dispatched", Order = 1)]
        public bool Dispatched { get; private set; }

        [DataMember(Name = "StoredAt", Order = 2)]
        public DateTimeOffset StoredAt { get; private set; }

        [DataMember(Name = "TransportOperations", Order = 3)]
        public StoredTransportOperation[] TransportOperations { get; private set; }

        public ExtensionDataObject ExtensionData { get; set; }

        public StoredOutboxMessage CloneAndMarkAsDispatched()
        {
            return new StoredOutboxMessage(Id, StoredAt, true);
        }

        bool Equals(StoredOutboxMessage other)
        {
            return string.Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is StoredOutboxMessage && Equals((StoredOutboxMessage) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        static StoredTransportOperation[] EmptyTransportOperations = new StoredTransportOperation[0];
    }

    [DataContract(Namespace = "NServiceBus.Persistence.ServiceFabric", Name = "StoredOutboxMessage")]
    class StoredTransportOperation : IExtensibleDataObject
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