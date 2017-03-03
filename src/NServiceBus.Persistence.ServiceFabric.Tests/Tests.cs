namespace NServiceBus.Persistence.ServiceFabric.Tests
{
    using System.Fabric;
    using TestRunner;

    sealed class Tests : AbstractTestRunner
    {
        public Tests(StatefulServiceContext context)
            : base(context)
        {
        }
    }
}