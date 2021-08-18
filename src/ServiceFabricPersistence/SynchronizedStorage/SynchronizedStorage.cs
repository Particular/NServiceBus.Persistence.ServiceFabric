namespace NServiceBus.Persistence.ServiceFabric
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Extensibility;
    using Microsoft.ServiceFabric.Data;

    class SynchronizedStorage : ISynchronizedStorage
    {
        readonly IReliableStateManager stateManager;
        readonly TimeSpan transactionTimeout;

        public SynchronizedStorage(IReliableStateManager stateManager, TimeSpan transactionTimeout)
        {
            this.transactionTimeout = transactionTimeout;
            this.stateManager = stateManager;
        }
        public Task<ICompletableSynchronizedStorageSession> OpenSession(ContextBag contextBag, CancellationToken cancellationToken = default)
        {
            var session = (ICompletableSynchronizedStorageSession)new StorageSession(stateManager, stateManager.CreateTransaction(), transactionTimeout, true);
            return Task.FromResult(session);
        }
    }
}