namespace NServiceBus.Persistence.ServiceFabric.Outbox
{
    using System;

    class CleanupStoredOutboxCommand
    {
        public string MessageId { get; set; }
        public DateTimeOffset StoredAt { get; set; }
    }
}