namespace NServiceBus.Persistence.ServiceFabric
{
    using System;

    /// <summary>
    /// Defines the collection name and/or the saga data name used for storing saga data.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class ServiceFabricSagaAttribute : Attribute
    {
        /// <summary>
        /// The saga data name should primarily be used to preserve backwards compatibility. By default, the saga data type
        /// name is used to create a deterministic identifier to store the saga in the reliable collection. Renaming a saga data
        /// class name will therefore change the saga data identifier. By setting the saga data name to saga data type
        /// name prior to before the renaming the identifier generation will remain stable.
        /// </summary>
        public string SagaDataName;

        /// <summary>
        /// The reliable collection name in which the saga data will be stored. By default each saga data will be stored
        /// in a collection named after the saga data type name.
        /// </summary>
        public string CollectionName;
    }
}