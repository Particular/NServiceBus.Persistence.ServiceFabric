namespace NServiceBus.Persistence.ServiceFabric
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Features;
    using Logging;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.ServiceFabric.Data;
    using Outbox;

    class OutboxPersistenceFeature : Feature
    {
        internal OutboxPersistenceFeature()
        {
            DependsOn<Outbox>();
            Defaults(s => s.EnableFeature(typeof(SynchronizedStorageFeature)));
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            var transactionTimeout = context.Settings.TransactionTimeout();
            var outboxStorage = new OutboxStorage(context.Settings.StateManager(), transactionTimeout);

            context.Services.AddSingleton<IOutboxStorage>(outboxStorage);

            var timeToKeepDeduplicationData = context.Settings.GetOrDefault<TimeSpan?>(TimeToKeepDeduplicationEntries) ?? TimeSpan.FromHours(1);

            var frequencyToRunDeduplicationDataCleanup = context.Settings.GetOrDefault<TimeSpan?>(FrequencyToRunDeduplicationDataCleanup) ?? TimeSpan.FromSeconds(30);

            context.RegisterStartupTask(new RegisterStores(context.Settings.StateManager(), outboxStorage));
            context.RegisterStartupTask(new OutboxCleaner(outboxStorage, timeToKeepDeduplicationData, frequencyToRunDeduplicationDataCleanup));
        }

        internal const string TimeToKeepDeduplicationEntries = "ServiceFabric.Persistence.Outbox.TimeToKeepDeduplicationEntries";
        internal const string FrequencyToRunDeduplicationDataCleanup = "ServiceFabric.Persistence.Outbox.FrequencyToRunDeduplicationDataCleanup";

        class OutboxCleaner : FeatureStartupTask
        {
            OutboxStorage storage;
            TimeSpan timeToKeepDeduplicationData;
            TimeSpan frequencyToRunDeduplicationDataCleanup;
            CancellationTokenSource tokenSource;
            Task cleanupTask;

            static ILog Logger = LogManager.GetLogger<OutboxCleaner>();

            public OutboxCleaner(OutboxStorage storage, TimeSpan timeToKeepDeduplicationData, TimeSpan frequencyToRunDeduplicationDataCleanup)
            {
                this.frequencyToRunDeduplicationDataCleanup = frequencyToRunDeduplicationDataCleanup;
                this.timeToKeepDeduplicationData = timeToKeepDeduplicationData;
                this.storage = storage;
            }

            protected override Task OnStart(IMessageSession session)
            {
                tokenSource = new CancellationTokenSource();

                cleanupTask = Task.Run(() => Cleanup(tokenSource.Token));

                return TaskEx.CompletedTask;
            }

            protected override Task OnStop(IMessageSession session)
            {
                tokenSource.Cancel();
                return cleanupTask;
            }

            async Task Cleanup(CancellationToken token)
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var now = DateTime.UtcNow;
                        var nextClean = now.Add(frequencyToRunDeduplicationDataCleanup);

                        var olderThan = now - timeToKeepDeduplicationData;

                        await storage.CleanUpOutboxQueue(olderThan, token).ConfigureAwait(false);

                        var delay = nextClean - now;
                        if (delay > TimeSpan.Zero)
                        {
                            await Task.Delay(delay, token).ConfigureAwait(false);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // graceful shutdown
                    }
                    catch (TimeoutException)
                    {
                        // happens on dead locks
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