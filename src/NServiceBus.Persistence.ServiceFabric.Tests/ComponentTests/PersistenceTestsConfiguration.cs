// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
namespace NServiceBus.Persistence.ComponentTests
{
    using System;
    using Extensibility;

    public partial class PersistenceTestsConfiguration : IPersistenceTestsConfiguration
    {
        public Func<ContextBag> GetContextBagForTimeoutPersister { get; set; } = () => new ContextBag();
        public Func<ContextBag> GetContextBagForSagaStorage { get; set; } = () => new ContextBag();
        public Func<ContextBag> GetContextBagForOutbox { get; set; } = () => new ContextBag();
        public Func<ContextBag> GetContextBagForSubscriptions { get; set; } = () => new ContextBag();
    }
}