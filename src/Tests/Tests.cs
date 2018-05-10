namespace NServiceBus.Persistence.ServiceFabric.Tests
{
    using System.Fabric;
    using global::TestRunner;

    sealed class Tests : AbstractTestRunner<Tests>
    {
        public Tests(StatefulServiceContext context)
            : base(context)
        {
        }

        protected override Tests Self => this;
    }
}