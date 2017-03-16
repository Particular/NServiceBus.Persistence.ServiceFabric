namespace NServiceBus.Persistence.ServiceFabric.Outbox
{
    using System;
    using System.Runtime.Serialization;

    [DataContract(Namespace = "NServiceBus.Persistence.ServiceFabric", Name = "CleanupStoredOutboxCommand")]
    sealed class CleanupStoredOutboxCommand : IExtensibleDataObject
    {
        public CleanupStoredOutboxCommand(string messageId, DateTimeOffset storedAt)
        {
            MessageId = messageId;
            StoredAt = storedAt;
        }

        [DataMember(Name = "MessageId", Order = 0)]
        public string MessageId { get; private set; }
        [DataMember(Name = "StoredAt", Order = 1)]
        public DateTimeOffset StoredAt { get; private set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }
}