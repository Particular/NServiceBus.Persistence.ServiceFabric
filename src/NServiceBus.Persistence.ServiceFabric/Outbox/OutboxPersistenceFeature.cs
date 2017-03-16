﻿namespace NServiceBus.Persistence.ServiceFabric.Outbox
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

            var timeToKeepDeduplicationData = context.Settings.GetOrDefault<TimeSpan?>(TimeToKeepDeduplicationEntries) ?? TimeSpan.FromDays(7);

            var frequencyToRunDeduplicationDataCleanup = context.Settings.GetOrDefault<TimeSpan?>(FrequencyToRunDeduplicationDataCleanup) ?? TimeSpan.FromMinutes(1);

            context.RegisterStartupTask(new RegisterStores(context.Settings.StateManager(), outboxStorage));
            context.RegisterStartupTask(new OutboxCleaner(outboxStorage, timeToKeepDeduplicationData, frequencyToRunDeduplicationDataCleanup));
        }

        internal const string TimeToKeepDeduplicationEntries = "ServiceFabric.Persistence.Outbox.TimeToKeepDeduplicationEntries";
        internal const string FrequencyToRunDeduplicationDataCleanup = "ServiceFabric.Persistence.Outbox.FrequencyToRunDeduplicationDataCleanup";

        class OutboxCleaner : FeatureStartupTask
        {
            readonly OutboxStorage storage;
            readonly TimeSpan timeToKeepDeduplicationData;
            readonly TimeSpan frequencyToRunDeduplicationDataCleanup;
            CancellationTokenSource _tokenSource;
            Task _cleanupTask;

            static ILog Logger = LogManager.GetLogger<OutboxCleaner>();

            public OutboxCleaner(OutboxStorage storage, TimeSpan timeToKeepDeduplicationData, TimeSpan frequencyToRunDeduplicationDataCleanup)
            {
                this.frequencyToRunDeduplicationDataCleanup = frequencyToRunDeduplicationDataCleanup;
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
                        var nextClean = DateTime.UtcNow.Add(frequencyToRunDeduplicationDataCleanup);

                        var olderThan = DateTime.UtcNow - timeToKeepDeduplicationData;
                        await storage.CleanupMessagesOlderThan(olderThan, token).ConfigureAwait(false);

                        var delay = nextClean - DateTime.UtcNow;
                        if (delay > TimeSpan.Zero)
                        {
                            await Task.Delay(delay, token).ConfigureAwait(false);
                        }
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
            public RegisterStores(IReliableStateManager stateManager, OutboxStorage storage)
            {
                outboxStorage = storage;
                this.stateManager = stateManager;
            }

            protected override Task OnStart(IMessageSession session)
            {
                return stateManager.RegisterOutboxStorage(outboxStorage);
            }

            protected override Task OnStop(IMessageSession session)
            {
                return TaskEx.CompletedTask;
            }

            IReliableStateManager stateManager;
            OutboxStorage outboxStorage;
        }
    }
}