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
            StoredOutboxMessage storedOutboxMessage;
            using (var tx = reliableStateManager.CreateTransaction())
            {
                var conditionalValue = await Outbox.TryGetValueAsync(tx, messageId).ConfigureAwait(false);
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
                transportOperations[i] = new TransportOperation(o.MessageId, o.Options, o.Body, o.Headers);
            }
            return new OutboxMessage(messageId, transportOperations);
        }

        public Task<OutboxTransaction> BeginTransaction(ContextBag context)
        {
            return Task.FromResult<OutboxTransaction>(new ServiceFabricOutboxTransaction(new ServiceFabricTransaction(reliableStateManager)));
        }

        public async Task Store(OutboxMessage message, OutboxTransaction transaction, ContextBag context)
        {
            var operations = new StoredTransportOperation[message.TransportOperations.Length];
            for (var i = 0; i < message.TransportOperations.Length; i++)
            {
                var t = message.TransportOperations[i];
                operations[i] = new StoredTransportOperation(t.MessageId, t.Options, t.Body, t.Headers);
            }

            var tx = ((ServiceFabricOutboxTransaction) transaction).Transaction.NativeTransaction;
            if (!await Outbox.TryAddAsync(tx, message.MessageId, new StoredOutboxMessage(message.MessageId, operations)).ConfigureAwait(false))
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
                    var storedOutboxMessage = conditionalValue.Value;

                    var dispatched = storedOutboxMessage.CloneAndMarkAsDispatched();

                    await Outbox.SetAsync(tx, messageId, dispatched).ConfigureAwait(false);

                    await Cleanup.EnqueueAsync(tx, new CleanupStoredOutboxCommand(messageId, storedOutboxMessage.StoredAt)).ConfigureAwait(false);
                }
                await tx.CommitAsync().ConfigureAwait(false);
            }
        }

        internal async Task CleanupMessagesOlderThan(DateTimeOffset date, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var tx = reliableStateManager.CreateTransaction())
            {
                var currentIndex = 0;
                var somethingToCommit = false;
                var cleanConditionalValue = await Cleanup.TryPeekAsync(tx, LockMode.Default, defaultOperationTimeout, cancellationToken).ConfigureAwait(false);

                while (cleanConditionalValue.HasValue && currentIndex <= 100)
                {
                    var cleanupCommand = cleanConditionalValue.Value;

                    if (cleanupCommand.StoredAt <= date)
                    {
                        await Outbox.TryRemoveAsync(tx, cleanupCommand.MessageId, defaultOperationTimeout, cancellationToken).ConfigureAwait(false);
                        await Cleanup.TryDequeueAsync(tx, defaultOperationTimeout, cancellationToken).ConfigureAwait(false);
                        somethingToCommit = true;
                    }
                    else
                    {
                        break;
                    }

                    currentIndex++;
                    cleanConditionalValue = await Cleanup.TryPeekAsync(tx, LockMode.Default, defaultOperationTimeout, cancellationToken).ConfigureAwait(false);
                }

                if (somethingToCommit)
                {
                    await tx.CommitAsync().ConfigureAwait(false);
                }
            }
        }
    }
}