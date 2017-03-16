namespace NServiceBus.Persistence.ServiceFabric.Outbox
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Features;
    using Logging;
    using Microsoft.ServiceFabric.Data;
    using NServiceBus.Outbox;

    class OutboxPersistenceFeature : Feature
    {
        internal OutboxPersistenceFeature()
        {
            DependsOn<Outbox>();
            Defaults(s => s.EnableFeature(typeof(SynchronizedStorageFeature)));
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            var outboxStorage = new OutboxStorage(context.Settings.StateManager());
            context.Container.RegisterSingleton<IOutboxStorage>(outboxStorage);

            var timeSpan = context.Settings.Get<TimeSpan>(TimeToKeepDeduplicationEntries);

            context.RegisterStartupTask(new RegisterStores(context.Settings.StateManager()));
            context.RegisterStartupTask(new OutboxCleaner(outboxStorage, timeSpan));
        }

        internal const string TimeToKeepDeduplicationEntries = "ServiceFabric.Persistence.Outbox.TimeToKeepDeduplicationEntries";

        class OutboxCleaner : FeatureStartupTask
        {
            readonly OutboxStorage storage;
            readonly TimeSpan timeToKeepDeduplicationData;
            CancellationTokenSource _tokenSource;
            Task _cleanupTask;

            static ILog Logger = LogManager.GetLogger<OutboxCleaner>();

            public OutboxCleaner(OutboxStorage storage, TimeSpan timeToKeepDeduplicationData)
            {
                this.timeToKeepDeduplicationData = timeToKeepDeduplicationData;
                this.storage = storage;
            }

            protected override Task OnStart(IMessageSession session)
            {
                _tokenSource = new CancellationTokenSource();

                _cleanupTask = Task.Run(() => Cleanup(_tokenSource.Token));

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
                    try
                    {
                        var dateTimeOffset = DateTimeOffset.UtcNow - timeToKeepDeduplicationData;
                        await storage.CleanupMessagesOlderThan(dateTimeOffset, token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        // graceful shutdown
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn("Unable to clean outbox storage.", ex);
                    }
                }
            }
        }

        class RegisterStores : FeatureStartupTask
        {
            public RegisterStores(IReliableStateManager stateManager)
            {
                this.stateManager = stateManager;
            }

            protected override Task OnStart(IMessageSession session)
            {
                return stateManager.RegisterOutboxStorage();
            }

            protected override Task OnStop(IMessageSession session)
            {
                return TaskEx.CompletedTask;
            }

            IReliableStateManager stateManager;
        }
    }
}