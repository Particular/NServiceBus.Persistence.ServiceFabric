namespace NServiceBus.Persistence.ServiceFabric.Outbox
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Features;
    using Microsoft.ServiceFabric.Data;
    using NServiceBus.Outbox;
    using SagaPersister;

    /// <summary>
    /// Used to configure in memory outbox persistence.
    /// </summary>
    public class ServiceFabricOutboxPersistence : Feature
    {
        internal ServiceFabricOutboxPersistence()
        {
            DependsOn<Outbox>();
            Defaults(s => s.EnableFeature(typeof(SynchronizedStorageFeature)));
        }

        /// <summary>
        /// See <see cref="Feature.Setup" />.
        /// </summary>
        protected override void Setup(FeatureConfigurationContext context)
        {
            var outboxStorage = new ServiceFabricOutboxStorage(context.Settings.StateManager());
            context.Container.RegisterSingleton<IOutboxStorage>(outboxStorage);

            var timeSpan = context.Settings.Get<TimeSpan>(TimeToKeepDeduplicationEntries);
            context.RegisterStartupTask(new OutboxCleaner(context.Settings.StateManager(), timeSpan));

            context.RegisterStartupTask(new RegisterStores(context.Settings.StateManager()));
        }

        internal const string TimeToKeepDeduplicationEntries = "ServiceFabric.Persistence.Outbox.TimeToKeepDeduplicationEntries";

        class OutboxCleaner : FeatureStartupTask
        {
            readonly IReliableStateManager _stateManager;
            readonly TimeSpan _timeToKeepDeduplicationData;
            CancellationTokenSource _tokenSource;
            Task _cleanupTask;

            public OutboxCleaner(IReliableStateManager stateManager, TimeSpan timeToKeepDeduplicationData)
            {
                _timeToKeepDeduplicationData = timeToKeepDeduplicationData;
                _stateManager = stateManager;
            }

            protected override Task OnStart(IMessageSession session)
            {
                _tokenSource = new CancellationTokenSource();

                _cleanupTask = Task.Factory.StartNew(() => Cleanup(_tokenSource.Token).GetAwaiter().GetResult() , TaskCreationOptions.LongRunning);

                return TaskEx.CompletedTask;
            }

            protected override Task OnStop(IMessageSession session)
            {
                _tokenSource.Cancel();
                return _cleanupTask;
            }

            async Task Cleanup(CancellationToken token)
            {
                while (!token.IsCancellationRequested)
                {
                    using (var tx = _stateManager.CreateTransaction())
                    {
                        var queue = await _stateManager.OutboxCleanup(tx);
                        var message = await queue.TryDequeueAsync(tx);
                        if (message.HasValue)
                        {
                            if (message.Value.StoredAt + _timeToKeepDeduplicationData <= DateTimeOffset.UtcNow)
                            {
                                var storage = await _stateManager.Outbox(tx);
                                await storage.TryRemoveAsync(tx, message.Value.MessageId);
                            }
                            else
                            {
                                await queue.EnqueueAsync(tx, new CleanupStoredOutboxCommand()
                                {
                                    MessageId = message.Value.MessageId,
                                    StoredAt = message.Value.StoredAt
                                });
                            }

                           await tx.CommitAsync();
                        }
                    }
                }
            }
        }

        internal class RegisterStores : FeatureStartupTask
        {
            public RegisterStores(IReliableStateManager stateManager)
            {
                this.stateManager = stateManager;
            }

            protected override Task OnStart(IMessageSession session)
            {
                return stateManager.RegisterDictionaries();
            }

            protected override Task OnStop(IMessageSession session)
            {
                return TaskEx.CompletedTask;
            }

            IReliableStateManager stateManager;
        }
    }
}