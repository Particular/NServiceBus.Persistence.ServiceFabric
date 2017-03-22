namespace NServiceBus.Persistence.ServiceFabric
{
    using System;

    /// <summary>
    /// Allows to influence the collection name and/or the saga data name when a saga is stored into reliable collections.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class ServiceFabricSagaAttribute : Attribute
    {
        /// <summary>
        /// The saga data name is primarily used for backwards compatibility. By default the saga data type name is used
        /// to create a determinstic identifier to store the saga into the reliable collection. Renaming a saga data
        /// will therefore influence the identifier generation. By setting the saga data name to saga data name before
        /// the renaming the identifier generation will rename stable.
        /// </summary>
        public string SagaDataName;

        /// <summary>
        /// The collection name allows to influence in which reliable collection the saga data will be stored. By default
        /// each saga data will be stored in a collection named after the saga data type name.
        /// </summary>
        public string CollectionName;
    }
}