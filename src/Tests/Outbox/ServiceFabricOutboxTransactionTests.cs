namespace NServiceBus.Persistence.ServiceFabric.Tests.Outbox
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using global::TestRunner;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;
    using NUnit.Framework;

    [TestFixture]
    public class ServiceFabricOutboxTransactionTests : INeed<IReliableStateManager>
    {
        public void Need(IReliableStateManager stateManager) => this.stateManager = stateManager;

        [SetUp]
        public void SetUp() => outboxTransaction = new ServiceFabricOutboxTransaction(stateManager);

        [Test]
        public async Task Commit_completes_transaction()
        {
            var dictionary = await stateManager.GetOrAddAsync<IReliableDictionary<string, string>>(Path.GetTempFileName(), TimeSpan.FromSeconds(5));
            await dictionary.AddAsync(outboxTransaction.Transaction, "Key", "Value");
            await outboxTransaction.Commit();

            using (var tx = stateManager.CreateTransaction())
            {
                var value = await dictionary.TryGetValueAsync(tx, "Key");

                Assert.True(value.HasValue);
                Assert.AreEqual("Value", value.Value);
            }
        }

        [Test]
        public async Task Dispose_transaction_owned_without_complete_rolls_back()
        {
            var dictionary = await stateManager.GetOrAddAsync<IReliableDictionary<string, string>>(Path.GetTempFileName(), TimeSpan.FromSeconds(5));
            await dictionary.AddAsync(outboxTransaction.Transaction, "Key", "Value");
            outboxTransaction.Dispose();

            using (var tx = stateManager.CreateTransaction())
            {
                var value = await dictionary.TryGetValueAsync(tx, "Key");

                Assert.False(value.HasValue);
            }
        }

        [TearDown]
        public void TearDown() => outboxTransaction.Dispose();

        IReliableStateManager stateManager;
        ServiceFabricOutboxTransaction outboxTransaction;
    }
}