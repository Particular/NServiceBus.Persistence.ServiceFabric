namespace TestRunner.Interfaces
{
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Services.Remoting;

    public interface ITestRunner : IService
    {
        Task<string[]> Tests();

        Task<Result> Run(string testName);
    }
}