namespace NServiceBus.Persistence.ServiceFabric
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Data;

    class StorageSession : ICompletableSynchronizedStorageSession, IServiceFabricStorageSession
    {
        public StorageSession(IReliableStateManager stateManager, ITransaction transaction, TimeSpan transactionTimeout, bool ownsTransaction)
        {
            this.ownsTransaction = ownsTransaction;
            StateManager = stateManager;
            Transaction = transaction;
            TransactionTimeout = transactionTimeout;
        }

        public void Dispose()
        {
            if (ownsTransaction)
            {
                Transaction.Dispose();
            }
        }

        public Task CompleteAsync(CancellationToken cancellationToken = default)
        {
            return ownsTransaction ? Transaction.CommitAsync() : Task.CompletedTask;
        }

        public IReliableStateManager StateManager { get; }

        public ITransaction Transaction { get; }

        public TimeSpan TransactionTimeout { get; }

        bool ownsTransaction;
    }
}