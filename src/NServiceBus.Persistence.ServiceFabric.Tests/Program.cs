using System.Threading;
using Microsoft.ServiceFabric.Services.Runtime;

namespace NServiceBus.Persistence.ServiceFabric.Tests
{
    internal static class Program
    {
        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static void Main()
        {

            ServiceRuntime.RegisterServiceAsync("TestsType",
                context => new Tests(context)).GetAwaiter().GetResult();

            // Prevents this host process from terminating so services keep running.
            Thread.Sleep(Timeout.Infinite);
        }
    }
}
