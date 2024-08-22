namespace NServiceBus.Persistence.ServiceFabric.Tests
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using global::TestRunner;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;
    using NUnit.Framework;
    using Testing;

    [TestFixture]
    public class TestableStorageSessionTests : INeed<IReliableStateManager>
    {
        public void Need(IReliableStateManager stateManager) => this.stateManager = stateManager;

        [Test]
        public async Task CanBeUsed()
        {
            string collectionName = Path.GetTempFileName();

            var dictionary = await stateManager.GetOrAddAsync<IReliableDictionary<string, string>>(collectionName, TimeSpan.FromSeconds(5));

            using (var tx = stateManager.CreateTransaction())
            {
                var testableSession =
                    new TestableServiceFabricStorageSession(stateManager, tx);
                var handlerContext = new TestableInvokeHandlerContext
                {
                    SynchronizedStorageSession = testableSession
                };
                handlerContext.Extensions.Set("CollectionName", collectionName);

                var handler = new HandlerUsingSynchronizedStorageSessionExtension();
                await handler.Handle(new MyMessage(), handlerContext);

                await tx.CommitAsync();
            }

            using (var tx = stateManager.CreateTransaction())
            {
                var value = await dictionary.TryGetValueAsync(tx, "Key");

                Assert.That(value.HasValue, Is.True);
                Assert.That(value.Value, Is.EqualTo("Value"));
            }
        }

        class HandlerUsingSynchronizedStorageSessionExtension : IHandleMessages<MyMessage>
        {
            public async Task Handle(MyMessage message, IMessageHandlerContext context)
            {
                var session = context.SynchronizedStorageSession.ServiceFabricSession();
                string collectionName = context.Extensions.Get<string>("CollectionName");
                var dictionary = await session.StateManager.GetOrAddAsync<IReliableDictionary<string, string>>(collectionName, TimeSpan.FromSeconds(5));
                await dictionary.AddAsync(session.Transaction, "Key", "Value");
            }
        }

        class MyMessage { }

        IReliableStateManager stateManager;
    }
}