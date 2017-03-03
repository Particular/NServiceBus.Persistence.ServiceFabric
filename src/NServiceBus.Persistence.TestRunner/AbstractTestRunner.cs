namespace TestRunner
{
    using System.Collections.Generic;
    using System.Fabric;
    using System.Threading.Tasks;
    using Interfaces;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Remoting.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;

    public abstract class AbstractTestRunner : StatefulService, ITestRunner
    {
        protected AbstractTestRunner(StatefulServiceContext context)
            : base(context)
        {

        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[] { new ServiceReplicaListener(context =>
            {
                communicationListener = new CommunicationListener<AbstractTestRunner>(this);
                return new CompositeCommunicationListener(communicationListener, this.CreateServiceRemotingListener(context));
            }) };
        }

        public Task<string[]> Tests()
        {
            return communicationListener.Tests();
        }

        public Task<Result> Run(string testName)
        {
            return communicationListener.Run(testName);
        }

        CommunicationListener<AbstractTestRunner> communicationListener;
    }
}