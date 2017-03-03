namespace NServiceBus.Persistence.ServiceFabric.AcceptanceTests
{
    using System.Fabric;
    using TestRunner;

    internal sealed class AcceptanceTests : AbstractTestRunner
    {
        public AcceptanceTests(StatefulServiceContext context)
            : base(context)
        {
        }
    }
}