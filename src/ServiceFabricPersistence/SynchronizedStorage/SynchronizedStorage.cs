namespace NServiceBus.Persistence.ServiceFabric
{
    using System;
    using System.Threading.Tasks;
    using Extensibility;
    using Microsoft.ServiceFabric.Data;

    class SynchronizedStorage : ISynchronizedStorage
    {
        IReliableStateManager stateManager;
        TimeSpan transactionTimeout;

        public SynchronizedStorage(IReliableStateManager stateManager, TimeSpan transactionTimeout)
        {
            this.transactionTimeout = transactionTimeout;
            this.stateManager = stateManager;
        }
        public Task<CompletableSynchronizedStorageSession> OpenSession(ContextBag contextBag)
        {
            var session = (CompletableSynchronizedStorageSession)new StorageSession(stateManager, stateManager.CreateTransaction(), transactionTimeout, true);
            return Task.FromResult(session);
        }
    }
}