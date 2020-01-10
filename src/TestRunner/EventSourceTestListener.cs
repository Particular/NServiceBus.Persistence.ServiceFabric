namespace NServiceBus.Persistence.TestRunner
{
    using NUnit.Framework.Interfaces;

    public class EventSourceTestListener : ITestListener
    {
        public void TestStarted(ITest test)
        {
        }

        public void TestFinished(ITestResult result)
        {
            if (!result.Test.IsSuite)
            {
                var message = $"Finished {result.FullName}. Status: {result.ResultState.Status}. Message: {result.Output}";
                logger.Information(message);
            }
        }

        public void TestOutput(TestOutput output)
        {
        }

        public void SendMessage(TestMessage message)
        {
        }

        EventSourceLogger logger = EventSourceLogger.GetLogger();
    }
}