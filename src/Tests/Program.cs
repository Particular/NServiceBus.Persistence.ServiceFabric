namespace NServiceBus.Persistence.ServiceFabric.Tests
{
    using System.Threading;
    using Microsoft.ServiceFabric.Services.Runtime;
    using Timeout = System.Threading.Timeout;

    static class Program
    {
        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        static void Main()
        {

            ServiceRuntime.RegisterServiceAsync("TestsType",
                context => new Tests(context)).GetAwaiter().GetResult();

            // Prevents this host process from terminating so services keep running.
            Thread.Sleep(Timeout.Infinite);
        }
    }
}
