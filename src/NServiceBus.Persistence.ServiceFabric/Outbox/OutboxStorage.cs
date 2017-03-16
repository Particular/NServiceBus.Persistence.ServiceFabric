namespace NServiceBus.Persistence.ServiceFabric.Outbox
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Extensibility;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;
    using NServiceBus.Outbox;

    class OutboxStorage : IOutboxStorage
    {
        IReliableStateManager reliableStateManager;
        static TimeSpan defaultOperationTimeout = TimeSpan.FromSeconds(4);

        public OutboxStorage(IReliableStateManager reliableStateManager)
        {
            this.reliableStateManager = reliableStateManager;
        }

        public IReliableDictionary<string, StoredOutboxMessage> Outbox { get; set; }

        public IReliableQueue<CleanupStoredOutboxCommand> Cleanup { get; set; }

        public async Task<OutboxMessage> Get(string messageId, ContextBag context)
        {
            OutboxMessage result = null;
            using (var tx = reliableStateManager.CreateTransaction())
            {
                var conditionalValue = await Outbox.TryGetValueAsync(tx, messageId).ConfigureAwait(false);
                if (conditionalValue.HasValue)
                {
                    result = new OutboxMessage(messageId, conditionalValue.Value.TransportOperations.Select(o => new TransportOperation(o.MessageId, o.Options, o.Body, o.Headers)).ToArray());
                }
            }
            return result;
        }

        public Task<OutboxTransaction> BeginTransaction(ContextBag context)
        {
            return Task.FromResult<OutboxTransaction>(new ServiceFabricOutboxTransaction(reliableStateManager));
        }

        public async Task Store(OutboxMessage message, OutboxTransaction transaction, ContextBag context)
        {
            var tx = ((ServiceFabricOutboxTransaction) transaction).Transaction;
            if (!await Outbox.TryAddAsync(tx, message.MessageId, new StoredOutboxMessage(message.MessageId, message.TransportOperations.Select(t => new StoredTransportOperation(t.MessageId, t.Options, t.Body, t.Headers)).ToArray())).ConfigureAwait(false))
            {
                throw new Exception($"Outbox message with id '{message.MessageId}' is already present in storage.");
            }
        }

        public async Task SetAsDispatched(string messageId, ContextBag context)
        {
            using (var tx = reliableStateManager.CreateTransaction())
            {
                var conditionalValue = await Outbox.TryGetValueAsync(tx, messageId).ConfigureAwait(false);
                if (conditionalValue.HasValue)
                {
                    var dispatched = conditionalValue.Value.CloneAndMarkAsDispatched();
                    await Outbox.SetAsync(tx, messageId, dispatched).ConfigureAwait(false);

                    var cleanup = await reliableStateManager.OutboxCleanup().ConfigureAwait(false);
                    await cleanup.EnqueueAsync(tx, new CleanupStoredOutboxCommand(messageId, conditionalValue.Value.StoredAt)).ConfigureAwait(false);
                }
                await tx.CommitAsync().ConfigureAwait(false);
            }
        }

        internal async Task CleanupMessagesOlderThan(DateTimeOffset date, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var tx = reliableStateManager.CreateTransaction())
            {
                var message = await Cleanup.TryDequeueAsync(tx, defaultOperationTimeout, cancellationToken).ConfigureAwait(false);
                if (message.HasValue)
                {
                    if (message.Value.StoredAt <= date)
                    {
                        await Outbox.TryRemoveAsync(tx, message.Value.MessageId, defaultOperationTimeout, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        await Cleanup.EnqueueAsync(tx, new CleanupStoredOutboxCommand(message.Value.MessageId, message.Value.StoredAt), defaultOperationTimeout, cancellationToken).ConfigureAwait(false);
                    }

                    await tx.CommitAsync().ConfigureAwait(false);
                }
            }
        }
    }
}