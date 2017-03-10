namespace NServiceBus.Persistence.ServiceFabric
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Data;

    class StorageSession : CompletableSynchronizedStorageSession, IServiceFabricStorageSession
    {
        Lazy<ITransaction> _transaction;

        public StorageSession(IReliableStateManager stateManager, Lazy<ITransaction> transaction)
        {
            StateManager = stateManager;
            _transaction = transaction;
        }

        public IReliableStateManager StateManager { get; }

        public ITransaction Transaction => _transaction.Value;

        public void Dispose()
        {
            if (_transaction.IsValueCreated)
            {
                _transaction.Value.Dispose();
            }
        }

        public Task CompleteAsync()
        {
            if (_transaction.IsValueCreated)
            {
                return _transaction.Value.CommitAsync();
            }
            return TaskEx.CompletedTask;
        }
    }
}