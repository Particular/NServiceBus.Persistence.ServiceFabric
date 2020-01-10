namespace NServiceBus.Persistence.ServiceFabric.Tests
{
    using System;
    using System.Threading.Tasks;
    using global::TestRunner;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;
    using NUnit.Framework;

    [TestFixture]
    public class StorageSessionTests : INeed<IReliableStateManager>
    {
        public void Need(IReliableStateManager stateManager)
        {
            this.stateManager = stateManager;
        }

        [SetUp]
        public void SetUp()
        {
            session = new StorageSession(stateManager, stateManager.CreateTransaction(), TimeSpan.FromSeconds(4), true);
        }

        [Test]
        public async Task CompleteAsync_completes_transaction()
        {
            var dictionary = await session.StateManager.GetOrAddAsync<IReliableDictionary<string, string>>("test", TimeSpan.FromSeconds(5));
            await dictionary.AddAsync(session.Transaction, "Key", "Value");
            await session.CompleteAsync();

            using (var tx = stateManager.CreateTransaction())
            {
                var value = await dictionary.TryGetValueAsync(tx, "Key");

                Assert.True(value.HasValue);
                Assert.AreEqual("Value", value.Value);
            }
        }

        [Test]
        public async Task CompleteAsync_completes_action_within_transaction()
        {
            var dictionary = await session.StateManager.GetOrAddAsync<IReliableDictionary<string, string>>("test1", TimeSpan.FromSeconds(5));
            await dictionary.AddAsync(session.Transaction, "Key", "Value");
            await session.CompleteAsync();

            using (var tx = stateManager.CreateTransaction())
            {
                var value = await dictionary.TryGetValueAsync(tx, "Key");

                Assert.True(value.HasValue);
                Assert.AreEqual("Value", value.Value);
            }
        }

        [Test]
        public async Task Dispose_without_complete_rolls_back()
        {
            var dictionary = await session.StateManager.GetOrAddAsync<IReliableDictionary<string, string>>("test2", TimeSpan.FromSeconds(5));
            await dictionary.AddAsync(session.Transaction, "Key", "Value");
            session.Dispose();

            using (var tx = stateManager.CreateTransaction())
            {
                var value = await dictionary.TryGetValueAsync(tx, "Key");

                Assert.False(value.HasValue);
            }
        }

        [Test]
        public async Task Dispose_without_complete_does_not_execute_actions()
        {
            var dictionary = await session.StateManager.GetOrAddAsync<IReliableDictionary<string, string>>("test3", TimeSpan.FromSeconds(5));
            await dictionary.AddAsync(session.Transaction, "Key", "Value");
            session.Dispose();

            using (var tx = stateManager.CreateTransaction())
            {
                var value = await dictionary.TryGetValueAsync(tx, "Key");

                Assert.False(value.HasValue);
            }
        }

        [TearDown]
        public void TearDown()
        {
            session.Dispose();
        }

        IReliableStateManager stateManager;
        StorageSession session;
    }
}