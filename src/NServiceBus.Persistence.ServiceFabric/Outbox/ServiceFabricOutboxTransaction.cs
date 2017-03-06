﻿namespace NServiceBus.Persistence.ServiceFabric.Outbox
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus.Outbox;

   // [SkipWeaving]
    class ServiceFabricOutboxTransaction : OutboxTransaction
    {
        public ServiceFabricOutboxTransaction()
        {
            Transaction = new ServiceFabricTransaction();
        }

        public ServiceFabricTransaction Transaction { get; private set; }

        public void Dispose()
        {
            Transaction = null;
        }

        public Task Commit()
        {
            Transaction.Commit();
            return Task.CompletedTask;
        }

        public void Enlist(Action action)
        {
            Transaction.Enlist(action);
        }
    }
}