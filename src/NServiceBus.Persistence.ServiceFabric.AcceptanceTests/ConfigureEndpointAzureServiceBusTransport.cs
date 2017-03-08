using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.AcceptanceTesting;
using NServiceBus.AcceptanceTesting.Support;
using NServiceBus.AcceptanceTests.ScenarioDescriptors;
using NServiceBus.MessageMutator;
using NServiceBus.Pipeline;

public class ConfigureEndpointAzureServiceBusTransport : IConfigureEndpointTestExecution
{
    public Task Configure(string endpointName, EndpointConfiguration configuration, RunSettings settings, PublisherMetadata publisherMetadata)
    {
        var connectionString = EnvironmentHelper.GetEnvironmentVariable("AzureServiceBus.ConnectionString");

        var transportConfig = configuration.UseTransport<AzureServiceBusTransport>();

        transportConfig.ConnectionString(connectionString);

        transportConfig.UseForwardingTopology();

        transportConfig.Sanitization().UseStrategy<ValidateAndHashIfNeeded>();

        configuration.RegisterComponents(c => { c.ConfigureComponent<TestIndependenceMutator>(DependencyLifecycle.SingleInstance); });

        configuration.Pipeline.Register("TestIndependenceBehavior", typeof(TestIndependenceSkipBehavior), "Skips messages not created during the current test.");

        // w/o retries ASB will move attempted messages to the error queue right away, which will cause false failure.
        // ScenarioRunner.PerformScenarios() verifies by default no messages are moved into error queue. If it finds any, it fails the test.
        configuration.Recoverability().Immediate(retriesSettings => retriesSettings.NumberOfRetries(3));

        return Task.FromResult(0);
    }

    public Task Cleanup()
    {
        return Task.FromResult(0);
    }
}

class TestIndependenceMutator : IMutateOutgoingTransportMessages
{
    string testRunId;

    public TestIndependenceMutator(ScenarioContext scenarioContext)
    {
        testRunId = scenarioContext.TestRunId.ToString();
    }

    public Task MutateOutgoing(MutateOutgoingTransportMessageContext context)
    {
        context.OutgoingHeaders["$AcceptanceTesting.TestRunId"] = testRunId;
        return Task.FromResult(0);
    }
}

class TestIndependenceSkipBehavior : Behavior<IIncomingPhysicalMessageContext>
{
    string testRunId;

    public TestIndependenceSkipBehavior(ScenarioContext scenarioContext)
    {
        testRunId = scenarioContext.TestRunId.ToString();
    }

    public override Task Invoke(IIncomingPhysicalMessageContext context, Func<Task> next)
    {
        string runId;
        if (!context.MessageHeaders.TryGetValue("$AcceptanceTesting.TestRunId", out runId) || runId != testRunId)
        {
            Console.WriteLine($"Skipping message {context.MessageId} from previous test run");
            return Task.FromResult(0);
        }

        return next();
    }
}