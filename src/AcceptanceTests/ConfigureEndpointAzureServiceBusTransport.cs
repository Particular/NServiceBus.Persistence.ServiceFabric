using System;
using System.Security.Cryptography;
using System.Text;
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
        var connectionStringName = $"{nameof(AzureServiceBusTransport)}.ConnectionString";

        var connectionString = EnvironmentHelper.GetEnvironmentVariable(connectionStringName);
        if (connectionString == null)
        {
            throw new Exception($"Environment variable with name {connectionStringName} must be provided.");
        }

        configuration.UseSerialization<XmlSerializer>();

        var transportConfig = configuration.UseTransport<AzureServiceBusTransport>();

        transportConfig.ConnectionString(connectionString);
        
        transportConfig.SubscriptionNameShortener(n => n.Length > MaxEntityName ? MD5DeterministicNameBuilder.Build(n) : n);
        transportConfig.RuleNameShortener(n => n.Length > MaxEntityName ? MD5DeterministicNameBuilder.Build(n) : n);

        configuration.RegisterComponents(c => { c.ConfigureComponent<TestIndependenceMutator>(DependencyLifecycle.SingleInstance); });

        configuration.Pipeline.Register("TestIndependenceBehavior", typeof(TestIndependenceSkipBehavior), "Skips messages not created during the current test.");

        // w/o retries ASB will move attempted messages to the error queue right away, which will cause false failure.
        // ScenarioRunner.PerformScenarios() verifies by default no messages are moved into error queue. If it finds any, it fails the test.
        configuration.Recoverability().Immediate(retriesSettings => retriesSettings.NumberOfRetries(3));

        return Task.FromResult(0);
    }
    
    const int MaxEntityName = 50;

    static class MD5DeterministicNameBuilder
    {
        public static string Build(string input)
        {
            var inputBytes = Encoding.Default.GetBytes(input);
            // use MD5 hash to get a 16-byte hash of the string
            using (var provider = new MD5CryptoServiceProvider())
            {
                var hashBytes = provider.ComputeHash(inputBytes);
                return new Guid(hashBytes).ToString();
            }
        }
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
        if (!context.MessageHeaders.TryGetValue("$AcceptanceTesting.TestRunId", out var runId) || runId != testRunId)
        {
            Console.WriteLine($"Skipping message {context.MessageId} from previous test run");
            return Task.FromResult(0);
        }

        return next();
    }
}