namespace NServiceBus.Persistence.ServiceFabric
{
    using System.Threading.Tasks;
    using Extensibility;
    using Microsoft.ServiceFabric.Data;

    class ServiceFabricSynchronizedStorage : ISynchronizedStorage
    {
        IReliableStateManager stateManager;

        public ServiceFabricSynchronizedStorage(IReliableStateManager stateManager)
        {
            this.stateManager = stateManager;
        }
        public Task<CompletableSynchronizedStorageSession> OpenSession(ContextBag contextBag)
        {
            var session = (CompletableSynchronizedStorageSession) new ServiceFabricSynchronizedStorageSession(stateManager);
            return Task.FromResult(session);
        }
    }
}