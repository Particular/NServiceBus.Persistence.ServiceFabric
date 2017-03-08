namespace NServiceBus.Persistence.ServiceFabric.Tests
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;
    using NUnit.Framework;
    using TestRunner;

    [TestFixture]
    public class SynchronizedStorageSessionTests : INeed<IReliableStateManager>
    {
        IReliableStateManager stateManager;
        SynchronizedStorageSession session;

        [SetUp]
        public void SetUp()
        {
            session = new SynchronizedStorageSession(stateManager);
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

        [TearDown]
        public void TearDown()
        {
            session.Dispose();
        }

        public void Need(IReliableStateManager dependency)
        {
            stateManager = dependency;
        }
    }
}