namespace NServiceBus.Persistence.ServiceFabric
{
    using System.Threading.Tasks;
    using Extensibility;
    using Microsoft.ServiceFabric.Data;

    class SynchronizedStorage : ISynchronizedStorage
    {
        IReliableStateManager stateManager;

        public SynchronizedStorage(IReliableStateManager stateManager)
        {
            this.stateManager = stateManager;
        }
        public Task<CompletableSynchronizedStorageSession> OpenSession(ContextBag contextBag)
        {
            var session = (CompletableSynchronizedStorageSession) new SynchronizedStorageSession(stateManager);
            return Task.FromResult(session);
        }
    }
}