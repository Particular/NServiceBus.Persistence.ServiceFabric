namespace NServiceBus.Persistence.ServiceFabric.Tests
{
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;
    using NUnit.Framework;
    using TestRunner;

    [TestFixture]
    public class TestAccessingReliableStateManager : INeed<IReliableStateManager>
    {
        IReliableStateManager stateManager;

        [Test]
        public async Task SomeTest()
        {
            var state = await stateManager.GetOrAddAsync<IReliableDictionary<string, string>>("state").ConfigureAwait(false);
            Assert.AreEqual("urn:somestate", state.Name);
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