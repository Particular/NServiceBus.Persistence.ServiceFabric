namespace TestRunner.Interfaces
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Services.Remoting;

    public interface ITestRunner : IService
    {
        Task<string[]> Tests(CancellationToken cancellationToken = default);

        Task<Result> Run(string testName, CancellationToken cancellationToken = default);
    }
}