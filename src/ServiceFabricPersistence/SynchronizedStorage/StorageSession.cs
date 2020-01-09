namespace NServiceBus.Persistence.ServiceFabric
{
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Data;

    class StorageSession : CompletableSynchronizedStorageSession, IServiceFabricStorageSession
    {
        public StorageSession(IReliableStateManager stateManager, ITransaction transaction, bool ownsTransaction)
        {
            this.ownsTransaction = ownsTransaction;
            StateManager = stateManager;
            Transaction = transaction;
        }

        public void Dispose()
        {
            if (ownsTransaction)
            {
                Transaction.Dispose();
            }
        }

        public Task CompleteAsync()
        {
            return ownsTransaction ? Transaction.CommitAsync() : TaskEx.CompletedTask;
        }

        public IReliableStateManager StateManager { get; }

        public ITransaction Transaction { get; }

        bool ownsTransaction;
    }
}