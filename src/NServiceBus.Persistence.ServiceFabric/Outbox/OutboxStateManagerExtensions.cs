namespace NServiceBus.Persistence.ServiceFabric.Outbox
{
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;

    static class OutboxStateManagerExtensions
    {
        public static async Task RegisterStores(this IReliableStateManager stateManager)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                await stateManager.Outbox(tx).ConfigureAwait(false);
                await stateManager.OutboxCleanup(tx).ConfigureAwait(false);

                await tx.CommitAsync().ConfigureAwait(false);
            }
        }

        public static Task<IReliableDictionary<string, StoredOutboxMessage>> Outbox(this IReliableStateManager stateManager, ITransaction transaction)
        {
            return stateManager.GetOrAddAsync<IReliableDictionary<string, StoredOutboxMessage>>(transaction, "outbox");
        }

        public static Task<IReliableQueue<CleanupStoredOutboxCommand>> OutboxCleanup(this IReliableStateManager stateManager, ITransaction transaction)
        {
            return stateManager.GetOrAddAsync<IReliableQueue<CleanupStoredOutboxCommand>>(transaction, "outboxCleanup");
        }
    }
}