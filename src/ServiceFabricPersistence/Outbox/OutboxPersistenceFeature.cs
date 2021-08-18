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
            readonly OutboxStorage storage;
            TimeSpan timeToKeepDeduplicationData;
            TimeSpan frequencyToRunDeduplicationDataCleanup;
            CancellationTokenSource tokenSource;
            Task cleanupTask;

            static readonly ILog Logger = LogManager.GetLogger<OutboxCleaner>();

            public OutboxCleaner(OutboxStorage storage, TimeSpan timeToKeepDeduplicationData, TimeSpan frequencyToRunDeduplicationDataCleanup)
            {
                this.frequencyToRunDeduplicationDataCleanup = frequencyToRunDeduplicationDataCleanup;
                this.timeToKeepDeduplicationData = timeToKeepDeduplicationData;
                this.storage = storage;
            }

            protected override Task OnStart(IMessageSession session, CancellationToken cancellationToken = default)
            {
                tokenSource = new CancellationTokenSource();

                // Task.Run() so the call returns immediately instead of waiting for the first await or return down the call stack
                cleanupTask = Task.Run(() => CleanupAndSwallowExceptions(tokenSource.Token), CancellationToken.None);

                return Task.CompletedTask;
            }

            protected override Task OnStop(IMessageSession session, CancellationToken cancellationToken = default)
            {
                tokenSource.Cancel();
                return cleanupTask;
            }

            async Task CleanupAndSwallowExceptions(CancellationToken cancellationToken)
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var now = DateTimeOffset.UtcNow;
                        var nextClean = now.Add(frequencyToRunDeduplicationDataCleanup);

                        var olderThan = now - timeToKeepDeduplicationData;

                        await storage.CleanUpOutboxQueue(olderThan, cancellationToken).ConfigureAwait(false);

                        var delay = nextClean - now;
                        if (delay > TimeSpan.Zero)
                        {
                            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                        }
                    }
                    catch (Exception ex) when (ex.IsCausedBy(cancellationToken))
                    {
                        // private token, feature is being stopped, log the exception in case the stack trace is ever needed for debugging
                        Logger.Debug("Operation canceled while stopping outbox persistence feature.", ex);
                        break;
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

            protected override Task OnStart(IMessageSession session, CancellationToken cancellationToken = default)
            {
                return stateManager.RegisterOutboxStorage(outboxStorage, cancellationToken);
            }

            protected override Task OnStop(IMessageSession session, CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }

            readonly IReliableStateManager stateManager;
            readonly OutboxStorage outboxStorage;
        }
    }
}
