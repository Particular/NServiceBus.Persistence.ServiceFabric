namespace NServiceBus.Persistence.ServiceFabric
{
    using System;
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
            var session = (CompletableSynchronizedStorageSession) new StorageSession(stateManager, new Lazy<ITransaction>(() => stateManager.CreateTransaction()));
            return Task.FromResult(session);
        }
    }
}