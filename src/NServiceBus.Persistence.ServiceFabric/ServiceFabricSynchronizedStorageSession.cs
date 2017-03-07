namespace NServiceBus.Persistence.ServiceFabric
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Data;
    using Persistence;

    class ServiceFabricSynchronizedStorageSession : CompletableSynchronizedStorageSession
    {
        Lazy<ITransaction> lazyTransaction;

        public ServiceFabricSynchronizedStorageSession(IReliableStateManager stateManager)
        {
            StateManager = stateManager;
            lazyTransaction = new Lazy<ITransaction>(() => StateManager.CreateTransaction());
        }

        public IReliableStateManager StateManager { get; }

        public ITransaction Transaction => lazyTransaction.Value;

        public void Dispose()
        {
            if (lazyTransaction.IsValueCreated)
            {
                lazyTransaction.Value.Dispose();
                lazyTransaction = null;
            }
        }

        public Task CompleteAsync()
        {
            if (lazyTransaction.IsValueCreated)
            {
                return lazyTransaction.Value.CommitAsync();
            }
            return Task.CompletedTask;
        }
    }
}