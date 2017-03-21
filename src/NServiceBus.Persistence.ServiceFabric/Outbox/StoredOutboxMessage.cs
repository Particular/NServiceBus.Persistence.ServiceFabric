namespace NServiceBus.Persistence.ServiceFabric
{
    using System;
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
}