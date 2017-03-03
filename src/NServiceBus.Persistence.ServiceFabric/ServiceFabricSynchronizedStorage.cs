namespace NServiceBus.Persistence.ServiceFabric
{
    using System.Threading.Tasks;
    using Extensibility;
    using Persistence;

    class ServiceFabricSynchronizedStorage : ISynchronizedStorage
    {
        public Task<CompletableSynchronizedStorageSession> OpenSession(ContextBag contextBag)
        {
            var session = (CompletableSynchronizedStorageSession) new ServiceFabricSynchronizedStorageSession();
            return Task.FromResult(session);
        }
    }
}