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
            OutboxMessage result;
            using (var tx = reliableStateManager.CreateTransaction())
            {
                var conditionalValue = await Outbox.TryGetValueAsync(tx, messageId).ConfigureAwait(false);
                if (!conditionalValue.HasValue)
                {
                    return null;
                }

                var storedOutboxMessage = conditionalValue.Value;
                var transportOperations = new TransportOperation[storedOutboxMessage.TransportOperations.Length];

                for (var i = 0; i < storedOutboxMessage.TransportOperations.Length; i++)
                {
                    var o = storedOutboxMessage.TransportOperations[i];
                    transportOperations[i] = new TransportOperation(o.MessageId, o.Options, o.Body, o.Headers);
                }

                result = new OutboxMessage(messageId, transportOperations);
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

            var operations = new StoredTransportOperation[message.TransportOperations.Length];
            for (var i = 0; i < message.TransportOperations.Length; i++)
            {
                var t = message.TransportOperations[i];
                operations[i] = new StoredTransportOperation(t.MessageId, t.Options, t.Body, t.Headers);
            }

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
                    var dispatched = conditionalValue.Value.CloneAndMarkAsDispatched();

                    await Outbox.SetAsync(tx, messageId, dispatched).ConfigureAwait(false);

                    await Cleanup.EnqueueAsync(tx, new CleanupStoredOutboxCommand(messageId, conditionalValue.Value.StoredAt)).ConfigureAwait(false);
                }
                await tx.CommitAsync().ConfigureAwait(false);
            }
        }

        internal async Task CleanupMessagesOlderThan(DateTimeOffset date, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var tx = reliableStateManager.CreateTransaction())
            {
                var iterator = await Cleanup.CreateEnumerableAsync(tx).ConfigureAwait(false);
                var enumerator = iterator.GetAsyncEnumerator();

                var currentIndex = 0;
                var somethingToCommit = false;
                while (await enumerator.MoveNextAsync(cancellationToken).ConfigureAwait(false) && currentIndex <= 100)
                {
                    var cleanupCommand = enumerator.Current;
                    if (cleanupCommand.StoredAt <= date)
                    {
                        await Outbox.TryRemoveAsync(tx, cleanupCommand.MessageId, defaultOperationTimeout, cancellationToken).ConfigureAwait(false);
                        somethingToCommit = true;
                    }
                    currentIndex++;
                }

                if (somethingToCommit)
                {
                    await tx.CommitAsync().ConfigureAwait(false);
                }
            }
        }
    }
}