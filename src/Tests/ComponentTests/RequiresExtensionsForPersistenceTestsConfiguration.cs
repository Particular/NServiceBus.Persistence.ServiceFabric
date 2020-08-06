namespace NServiceBus.Persistence.ComponentTests
{
    using NUnit.Framework;

    public static class RequiresExtensionsForPersistenceTestsConfiguration
    {
        public static void RequiresSubscriptionSupport(this IPersistenceTestsConfiguration configuration)
        {
            if (!configuration.SupportsSubscriptions)
            {
                Assert.Ignore("Ignoring this test because it requires subscription support from persister.");
            }
        }

        public static void RequiresTimeoutSupport(this IPersistenceTestsConfiguration configuration)
        {
            if (!configuration.SupportsTimeouts)
            {
                Assert.Ignore("Ignoring this test because it requires timout support from persister.");
            }
        }
    }
}