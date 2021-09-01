namespace NServiceBus.Persistence.ServiceFabric
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Extensibility;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;
    using Outbox;

    class OutboxStorage : IOutboxStorage
    {
        readonly IReliableStateManager reliableStateManager;
        readonly TimeSpan transactionTimeout;

        public OutboxStorage(IReliableStateManager reliableStateManager, TimeSpan transactionTimeout)
        {
            this.transactionTimeout = transactionTimeout;
            this.reliableStateManager = reliableStateManager;
        }

        public IReliableDictionary<string, StoredOutboxMessage> Outbox { get; set; }

        public IReliableQueue<CleanupStoredOutboxCommand> CleanupOld { get; set; }
        public IReliableConcurrentQueue<CleanupStoredOutboxCommand> Cleanup { get; set; }

        public async Task<OutboxMessage> Get(string messageId, ContextBag context, CancellationToken cancellationToken = default)
        {
            StoredOutboxMessage storedOutboxMessage;
            using (var tx = reliableStateManager.CreateTransaction())
            {
                var conditionalValue = await Outbox.TryGetValueAsync(tx, messageId, transactionTimeout, cancellationToken).ConfigureAwait(false);
                if (!conditionalValue.HasValue)
                {
                    return null;
                }

                storedOutboxMessage = conditionalValue.Value;
            }

            var transportOperations = new TransportOperation[storedOutboxMessage.TransportOperations.Length];
            for (var i = 0; i < storedOutboxMessage.TransportOperations.Length; i++)
            {
                var o = storedOutboxMessage.TransportOperations[i];
                transportOperations[i] = new TransportOperation(o.MessageId, new Transport.DispatchProperties(o.Options), o.Body, o.Headers);
            }
            return new OutboxMessage(messageId, transportOperations);
        }

        public Task<IOutboxTransaction> BeginTransaction(ContextBag context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IOutboxTransaction>(new ServiceFabricOutboxTransaction(reliableStateManager, reliableStateManager.CreateTransaction(), transactionTimeout));
        }

        public async Task Store(OutboxMessage message, IOutboxTransaction transaction, ContextBag context, CancellationToken cancellationToken = default)
        {
            var operations = new StoredTransportOperation[message.TransportOperations.Length];
            for (var i = 0; i < message.TransportOperations.Length; i++)
            {
                var t = message.TransportOperations[i];
                operations[i] = new StoredTransportOperation(t.MessageId, t.Options, t.Body.ToArray(), t.Headers);
            }

            var tx = ((ServiceFabricOutboxTransaction)transaction).Transaction;
            if (!await Outbox.TryAddAsync(tx, message.MessageId, new StoredOutboxMessage(message.MessageId, operations), transactionTimeout, cancellationToken).ConfigureAwait(false))
            {
                throw new Exception($"Outbox message with id '{message.MessageId}' is already present in storage.");
            }
        }

        public async Task SetAsDispatched(string messageId, ContextBag context, CancellationToken cancellationToken = default)
        {
            using (var tx = reliableStateManager.CreateTransaction())
            {
                var conditionalValue = await Outbox.TryGetValueAsync(tx, messageId, transactionTimeout, cancellationToken).ConfigureAwait(false);
                if (conditionalValue.HasValue)
                {
                    var storedOutboxMessage = conditionalValue.Value;

                    var dispatched = storedOutboxMessage.CloneAndMarkAsDispatched();

                    await Outbox.SetAsync(tx, messageId, dispatched, transactionTimeout, cancellationToken).ConfigureAwait(false);

                    await Cleanup.EnqueueAsync(tx, new CleanupStoredOutboxCommand(messageId, storedOutboxMessage.StoredAt), cancellationToken).ConfigureAwait(false);
                }
                await tx.CommitAsync().ConfigureAwait(false);
            }
        }

        internal Task CleanUpOutboxQueue(DateTimeOffset olderThan, CancellationToken cancellationToken = default)
        {
            // Both, the old and the new queues are cleaned up. This ensures that under no circumstance something is left over in the old outbox
            // The operational lock on the old queue should be short lived and should not collide with anything (there's no longer an active producer that enqueues to this queue).
            return Task.WhenAll(
                CleanUpOldOutboxQueue(olderThan, cancellationToken),
                CleanUpNewOutboxQueue(olderThan, cancellationToken));
        }

        async Task CleanUpOldOutboxQueue(DateTimeOffset olderThan, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var tx = reliableStateManager.CreateTransaction())
            {
                var currentIndex = 0;

                var cleanConditionalValue = await CleanupOld.TryPeekAsync(tx, LockMode.Default, transactionTimeout, cancellationToken).ConfigureAwait(false);

                while (cleanConditionalValue.HasValue && currentIndex <= 100)
                {
                    var cleanupCommand = cleanConditionalValue.Value;

                    if (cleanupCommand.StoredAt <= olderThan)
                    {
                        await Outbox.TryRemoveAsync(tx, cleanupCommand.MessageId, transactionTimeout, cancellationToken).ConfigureAwait(false);
                        await CleanupOld.TryDequeueAsync(tx, transactionTimeout, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        break;
                    }

                    currentIndex++;
                    cleanConditionalValue = await CleanupOld.TryPeekAsync(tx, LockMode.Default, transactionTimeout, cancellationToken).ConfigureAwait(false);
                }

                await tx.CommitAsync().ConfigureAwait(false);
            }
        }

        async Task CleanUpNewOutboxQueue(DateTimeOffset olderThan, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var tx = reliableStateManager.CreateTransaction())
            {
                var currentIndex = 0;
                var somethingToCommit = false;

                var cleanConditionalValue = await Cleanup.TryDequeueAsync(tx, cancellationToken, transactionTimeout).ConfigureAwait(false);

                while (cleanConditionalValue.HasValue && currentIndex <= 100)
                {
                    var cleanupCommand = cleanConditionalValue.Value;

                    if (cleanupCommand.StoredAt <= olderThan)
                    {
                        await Outbox.TryRemoveAsync(tx, cleanupCommand.MessageId, transactionTimeout, cancellationToken).ConfigureAwait(false);
                        somethingToCommit = true;
                    }
                    else
                    {
                        if (somethingToCommit)
                        {
                            // there's something to commit. The last cleanupCommand is enqueued again and the whole batch is committed
                            await Cleanup.EnqueueAsync(tx, cleanupCommand, cancellationToken, transactionTimeout).ConfigureAwait(false);
                            await tx.CommitAsync().ConfigureAwait(false);
                            return;
                        }

                        // nothing to commit, make the message reappear
                        tx.Abort();
                        return;
                    }

                    currentIndex++;
                    cleanConditionalValue = await Cleanup.TryDequeueAsync(tx, cancellationToken, transactionTimeout).ConfigureAwait(false);
                }

                await tx.CommitAsync().ConfigureAwait(false);
            }
        }
    }
}