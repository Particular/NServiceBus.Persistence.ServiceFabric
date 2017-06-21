namespace NServiceBus.Persistence.ServiceFabric
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Data;

    class StorageSession : CompletableSynchronizedStorageSession, IServiceFabricStorageSession
    {
        public StorageSession(ServiceFabricTransaction transaction) : this(transaction, false)
        {
        }

        public StorageSession(IReliableStateManager stateManager) : this(new ServiceFabricTransaction(stateManager), true)
        {
        }

        StorageSession(ServiceFabricTransaction transaction, bool ownsTransaction)
        {
            this.ownsTransaction = ownsTransaction;
            this.transaction = transaction;
        }

        public void Dispose()
        {
            if (ownsTransaction)
            {
                transaction.Dispose();
            }
        }

        public Task CompleteAsync()
        {
            return ownsTransaction ? transaction.Commit() : TaskEx.CompletedTask;
        }

        public IReliableStateManager StateManager => transaction.StateManager;

        public ITransaction Transaction => transaction.NativeTransaction;

        public Task Add(Func<ITransaction, Task> action)
        {
            return transaction.Add(action);
        }

        ServiceFabricTransaction transaction;
        bool ownsTransaction;
    }
}