namespace NServiceBus.Persistence.ServiceFabric.Tests
{
    using System;
    using System.Threading.Tasks;
    using global::TestRunner;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;
    using NUnit.Framework;

    [TestFixture]
    public class TestAccessingReliableStateManager : INeed<IReliableStateManager>
    {
        IReliableStateManager stateManager;

        [Test]
        public async Task SomeTest()
        {
            var state = await stateManager.GetOrAddAsync<IReliableDictionary<string, string>>("state", TimeSpan.FromSeconds(30)).ConfigureAwait(false);
            Assert.AreEqual("urn:state", state.Name.ToString());
        }

        [TearDown]
        public void TearDown()
        {

        }

        public void Need(IReliableStateManager dependency)
        {
            stateManager = dependency;
        }
    }
}