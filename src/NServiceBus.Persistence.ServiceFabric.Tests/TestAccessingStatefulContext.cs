namespace NServiceBus.Persistence.ServiceFabric.Tests
{
    using System;
    using System.Fabric;
    using NUnit.Framework;
    using TestRunner;

    [TestFixture]
    public class TestAccessingStatefulContext : INeed<StatefulServiceContext>
    {
        public void Need(StatefulServiceContext dependency)
        {
            statefulServiceContext = dependency;
        }

        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void SomeTest()
        {
            Console.WriteLine(statefulServiceContext.ServiceName);
            Console.WriteLine(statefulServiceContext.ReplicaId);
            Assert.AreEqual("TestsType", statefulServiceContext.ServiceTypeName);
        }

        [TearDown]
        public void TearDown()
        {
        }

        private StatefulServiceContext statefulServiceContext;
    }
}