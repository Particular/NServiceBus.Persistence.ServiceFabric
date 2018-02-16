namespace NServiceBus.Persistence.ComponentTests
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using global::TestRunner;
    using Microsoft.ServiceFabric.Data;
    using NUnit.Framework;
    using Outbox;
    using ServiceFabric;

    [TestFixture]
    public class OutboxStorageServiceFabricSpecificTests : INeed<IReliableStateManager>

    {
        PersistenceTestsConfiguration configuration;
        IReliableStateManager stateManager;

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            configuration = new PersistenceTestsConfiguration();
            await configuration.Configure();
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            await configuration.Cleanup();
        }

        [Test]
        public async Task Should_clear_dispatched_messages_and_cleanup_queue_after_given_expiry()
        {
            configuration.RequiresOutboxSupport();

            var storage = (OutboxStorage) configuration.OutboxStorage;
            var ctx = configuration.GetContextBagForOutbox();

            var messageId = Guid.NewGuid().ToString();

            var messageToStore = new OutboxMessage(messageId, new[] {new TransportOperation("x", null, null, null)});
            using (var transaction = await storage.BeginTransaction(ctx))
            {
                await storage.Store(messageToStore, transaction, ctx);

                await transaction.Commit();
            }

            // Account for the low resolution of DateTime.UtcNow.
            var afterStore = DateTimeOffset.UtcNow.AddTicks(1);

            await storage.SetAsDispatched(messageId, configuration.GetContextBagForOutbox());

            await configuration.CleanupMessagesOlderThan(afterStore);

            var message = await storage.Get(messageId, configuration.GetContextBagForOutbox());
            using (var tx = stateManager.CreateTransaction())
            {
                Assert.AreEqual(0, await storage.Cleanup.GetCountAsync(tx));
                Assert.AreEqual(0, await storage.Outbox.GetCountAsync(tx));
            }

            Assert.Null(message);
        }

        [Test]
        public async Task Should_clear_dispatched_messages_and_cleanup_queue_after_given_expiry_even_under_high_concurrency()
        {
            configuration.RequiresOutboxSupport();

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));

            var storage = (OutboxStorage)configuration.OutboxStorage;
            var ctx = configuration.GetContextBagForOutbox();

            var cleanupTask = Task.Run(async () =>
            {
                using (var tx = stateManager.CreateTransaction())
                {
                    await storage.Cleanup.GetCountAsync(tx);
                }

                long cleanupCount = 1;
                while (!cts.IsCancellationRequested && cleanupCount > 0)
                {
                    await configuration.CleanupMessagesOlderThan(DateTimeOffset.UtcNow);

                    using (var tx = stateManager.CreateTransaction())
                    {
                        cleanupCount = await storage.Cleanup.GetCountAsync(tx) + await storage.Outbox.GetCountAsync(tx);
                    }
                }

                return cleanupCount;
            }, CancellationToken.None);

            var inserts = Enumerable.Repeat(1, 200).Select(async _ =>
            {
                var messageId = Guid.NewGuid().ToString();

                var messageToStore = new OutboxMessage(messageId, new[] {new TransportOperation("x", null, null, null)});
                using (var transaction = await storage.BeginTransaction(ctx))
                {
                    await storage.Store(messageToStore, transaction, ctx);

                    await transaction.Commit();
                }

                await storage.SetAsDispatched(messageId, configuration.GetContextBagForOutbox());
            });

            await Task.WhenAll(inserts);

            Assert.AreEqual(0, await cleanupTask);
        }

        public void Need(IReliableStateManager dependency)
        {
            stateManager = dependency;
        }
    }
}