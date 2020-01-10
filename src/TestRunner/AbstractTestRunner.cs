namespace TestRunner
{
    using System.Collections.Generic;
    using System.Fabric;
    using System.Linq;
    using System.Threading.Tasks;
    using Interfaces;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Remoting.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;

    public abstract class AbstractTestRunner<TSelf> : StatefulService, ITestRunner
        where TSelf : StatefulService
    {
        protected AbstractTestRunner(StatefulServiceContext context)
            : base(context)
        {
        }

        protected abstract TSelf Self { get; }

        public Task<string[]> Tests()
        {
            return communicationListener.Tests();
        }

        public Task<Result> Run(string testName)
        {
            return communicationListener.Run(testName);
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            var localListeners = new[]
            {
                new ServiceReplicaListener(context =>
                {
                    communicationListener = new CommunicationListener<TSelf>(Self);
                    return communicationListener;
                }, "CommunicationListener")
            };
            return this.CreateServiceRemotingReplicaListeners().Concat(localListeners);
        }

        CommunicationListener<TSelf> communicationListener;
    }
}