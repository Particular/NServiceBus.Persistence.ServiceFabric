namespace TestRunner
{
    using Microsoft.ServiceFabric.Services.Runtime;
    using NUnit.Framework.Interfaces;

    class StatefulServiceProviderListener<TService> : ITestListener
        where TService : StatefulService
    {
        public StatefulServiceProviderListener(TService service)
        {
            this.service = service;
        }

        public void TestStarted(ITest test)
        {
            test.Properties.Set("StatefulService", service);
        }

        public void TestFinished(ITestResult result)
        {
        }

        public void TestOutput(TestOutput output)
        {
        }

        TService service;
    }
}