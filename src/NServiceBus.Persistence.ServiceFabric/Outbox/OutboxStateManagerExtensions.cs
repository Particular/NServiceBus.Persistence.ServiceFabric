namespace NServiceBus.Persistence.ServiceFabric
{
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;

    static class OutboxStateManagerExtensions
    {
        public static async Task RegisterOutboxStorage(this IReliableStateManager stateManager, OutboxStorage storage)
        {
            storage.Outbox = await stateManager.GetOrAddAsync<IReliableDictionary<string, StoredOutboxMessage>>("outbox").ConfigureAwait(false);

            storage.Cleanup = await stateManager.GetOrAddAsync<IReliableQueue<CleanupStoredOutboxCommand>>("outboxCleanup").ConfigureAwait(false);
        }
    }
}