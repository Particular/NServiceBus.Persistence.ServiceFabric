namespace TestHarness
{
    public class AcceptanceTests : R<AcceptanceTests>
    {
        public static string[] EnvironmentVariables { get; set; } = { "AzureServiceBus.ConnectionString" };
    }
}