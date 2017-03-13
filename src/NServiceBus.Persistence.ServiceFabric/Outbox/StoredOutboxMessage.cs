namespace NServiceBus.Persistence.ServiceFabric.Outbox
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract(Namespace = "NServiceBus.Persistence.ServiceFabric", Name = "StoredOutboxMessage")]
    class StoredOutboxMessage
    {
        public StoredOutboxMessage(string messageId, StoredTransportOperation[] transportOperations)
        {
            TransportOperations = transportOperations;
            Id = messageId;
            StoredAt = DateTimeOffset.UtcNow;
        }

        StoredOutboxMessage(string messageId, DateTimeOffset storedAt, bool dispatched)
        {
            TransportOperations = new StoredTransportOperation[0];
            Id = messageId;
            StoredAt = storedAt;
            Dispatched = dispatched;
        }

        [DataMember(Name = "Id", Order = 0)]
        public string Id { get; }

        [DataMember(Name = "Dispatched", Order = 1)]
        public bool Dispatched { get; }

        [DataMember(Name = "StoredAt", Order = 2)]
        public DateTimeOffset StoredAt { get; }

        public StoredTransportOperation[] TransportOperations { get; }

        public StoredOutboxMessage CloneAndMarkAsDispatched()
        {
            return new StoredOutboxMessage(Id, StoredAt, true);
        }

        protected bool Equals(StoredOutboxMessage other)
        {
            return string.Equals(Id, other.Id) && Dispatched.Equals(other.Dispatched);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((StoredOutboxMessage)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Id?.GetHashCode() ?? 0) * 397) ^ Dispatched.GetHashCode();
            }
        }
    }

    [DataContract(Namespace = "NServiceBus.Persistence.ServiceFabric", Name = "StoredOutboxMessage")]
    class StoredTransportOperation
    {
        [DataMember(Name = "MessageId", Order = 0)]
        public string MessageId { get; private set; }

        [DataMember(Name = "Options", Order = 1)]
        public Dictionary<string, string> Options { get; private set; }

        [DataMember(Name = "Body", Order = 2)]
        public byte[] Body { get; private set; }

        [DataMember(Name = "Headers", Order = 3)]
        public Dictionary<string, string> Headers { get; private set; }
          
        public StoredTransportOperation(string messageId, Dictionary<string, string> options, byte[] body, Dictionary<string, string> headers)
        {
            this.MessageId = messageId;
            this.Options = options;
            this.Body = body;
            this.Headers = headers;
        }
    }
}