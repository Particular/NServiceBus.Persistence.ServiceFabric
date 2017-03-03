namespace NServiceBus.Persistence.ServiceFabric
{
    using System;
    using System.Threading.Tasks;
    using Persistence;

   // [SkipWeaving]
    class ServiceFabricSynchronizedStorageSession : CompletableSynchronizedStorageSession
    {
        public ServiceFabricSynchronizedStorageSession(ServiceFabricTransaction transaction)
        {
            Transaction = transaction;
        }

        public ServiceFabricSynchronizedStorageSession()
            : this(new ServiceFabricTransaction())
        {
            ownsTransaction = true;
        }

        public ServiceFabricTransaction Transaction { get; private set; }

        public void Dispose()
        {
            Transaction = null;
        }

        public Task CompleteAsync()
        {
            if (ownsTransaction)
            {
                Transaction.Commit();
            }
            return Task.CompletedTask;
        }

        public void Enlist(Action action)
        {
            Transaction.Enlist(action);
        }

        bool ownsTransaction;
    }
}