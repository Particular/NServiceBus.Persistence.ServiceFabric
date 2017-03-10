namespace NServiceBus.Persistence.ServiceFabric.SagaPersister
{
    using System;
    using System.Runtime.Serialization;

    [DataContract(Namespace = "NServiceBus.Persistence.ServiceFabric", Name = "CorrelationPropertyEntry")]
    internal class CorrelationPropertyEntry : IExtensibleDataObject, IComparable<CorrelationPropertyEntry>, IEquatable<CorrelationPropertyEntry>
    {
        [DataMember(Name = "SagaDataType", Order = 0)]
        public string SagaDataType { get; set; }

        [DataMember(Name = "Name", Order = 1)]
        public string Name { get; set; }

        [DataMember(Name = "Value", Order = 2)]
        public string Value { get; set; }

        [DataMember(Name = "Type", Order = 3)]
        public string Type { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }

        public int CompareTo(CorrelationPropertyEntry other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            var sagaDataTypeComparison = string.Compare(SagaDataType, other.SagaDataType, StringComparison.OrdinalIgnoreCase);
            if (sagaDataTypeComparison != 0) return sagaDataTypeComparison;
            var nameComparison = string.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase);
            if (nameComparison != 0) return nameComparison;
            var valueComparison = string.Compare(Value, other.Value, StringComparison.OrdinalIgnoreCase);
            if (valueComparison != 0) return valueComparison;
            return string.Compare(Type, other.Type, StringComparison.OrdinalIgnoreCase);
        }

        public bool Equals(CorrelationPropertyEntry other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(SagaDataType, other.SagaDataType, StringComparison.OrdinalIgnoreCase) && string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase) && string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase) && string.Equals(Type, other.Type, StringComparison.OrdinalIgnoreCase) && Equals(ExtensionData, other.ExtensionData);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CorrelationPropertyEntry) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (SagaDataType != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(SagaDataType) : 0);
                hashCode = (hashCode * 397) ^ (Name != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(Name) : 0);
                hashCode = (hashCode * 397) ^ (Value != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(Value) : 0);
                hashCode = (hashCode * 397) ^ (Type != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(Type) : 0);
                hashCode = (hashCode * 397) ^ (ExtensionData != null ? ExtensionData.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}