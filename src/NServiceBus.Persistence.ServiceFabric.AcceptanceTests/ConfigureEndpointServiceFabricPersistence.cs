using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Runtime;
using NServiceBus;
using NServiceBus.AcceptanceTesting.Support;
using NServiceBus.Configuration.AdvanceExtensibility;
using NServiceBus.Persistence.ServiceFabric;
using NUnit.Framework;

public class ConfigureEndpointServiceFabricPersistence : IConfigureEndpointTestExecution
{
    public Task Configure(string endpointName, EndpointConfiguration configuration, RunSettings settings, PublisherMetadata publisherMetadata)
    {
        if (configuration.GetSettings().Get<bool>("Endpoint.SendOnly"))
        {
            return Task.FromResult(0);
        }

        var statefulService = (StatefulService) TestContext.CurrentContext.Test.Properties.Get("StatefulService");
        var persistence = configuration.UsePersistence<ServiceFabricPersistence>();
        persistence.StateManager(statefulService.StateManager);
        return Task.FromResult(0);
    }

    public Task Cleanup()
    {
        return Task.FromResult(0);
    }
}