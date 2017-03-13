namespace NServiceBus.Persistence.ServiceFabric.Outbox
{
    using System;
    using System.Runtime.Serialization;

    [DataContract(Namespace = "NServiceBus.Persistence.ServiceFabric", Name = "CleanupStoredOutboxCommand")]
    class CleanupStoredOutboxCommand
    {
        [DataMember(Name = "MessageId", Order = 0)]
        public string MessageId { get; set; }
        [DataMember(Name = "StoredAt", Order = 1)]
        public DateTimeOffset StoredAt { get; set; }
    }
}