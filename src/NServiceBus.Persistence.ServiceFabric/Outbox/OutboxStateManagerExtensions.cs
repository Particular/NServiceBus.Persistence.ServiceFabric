namespace NServiceBus.Persistence.ServiceFabric.Outbox
{
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;

    static class OutboxStateManagerExtensions
    {
        public static async Task RegisterOutboxStorage(this IReliableStateManager stateManager, OutboxStorage storage)
        {
            var outbox = await stateManager.Outbox().ConfigureAwait(false);
            storage.Outbox = outbox;

            var cleanup = await stateManager.OutboxCleanup().ConfigureAwait(false);
            storage.Cleanup = cleanup;
        }

        public static Task<IReliableDictionary<string, StoredOutboxMessage>> Outbox(this IReliableStateManager stateManager)
        {
            return stateManager.GetOrAddAsync<IReliableDictionary<string, StoredOutboxMessage>>("outbox");
        }

        public static Task<IReliableQueue<CleanupStoredOutboxCommand>> OutboxCleanup(this IReliableStateManager stateManager)
        {
            return stateManager.GetOrAddAsync<IReliableQueue<CleanupStoredOutboxCommand>>("outboxCleanup");
        }
    }
}