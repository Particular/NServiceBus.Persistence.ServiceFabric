namespace NServiceBus.Persistence.ServiceFabric.AcceptanceTests
{
    using System.Fabric;
    using TestRunner;

    sealed class AcceptanceTests : AbstractTestRunner<AcceptanceTests>
    {
        public AcceptanceTests(StatefulServiceContext context)
            : base(context)
        {
        }

        protected override AcceptanceTests Self => this;
    }
}