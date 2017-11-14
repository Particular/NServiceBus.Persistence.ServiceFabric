namespace NServiceBus.Persistence.ServiceFabric
{
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using Newtonsoft.Json;
    using Sagas;

    class SagaIdGenerator : ISagaIdGenerator
    {
        SagaInfoCache sagaInfoCache;

        public void Initialize(SagaInfoCache sagaInfoCache)
        {
            this.sagaInfoCache = sagaInfoCache;
        }

        public Guid Generate(SagaIdGeneratorContext context)
        {
            if (context.CorrelationProperty == SagaCorrelationProperty.None)
            {
                return Guid.NewGuid();
            }

            var sagaInfo = sagaInfoCache.GetInfo(context.SagaMetadata.SagaEntityType);

            return Generate(sagaInfo, context.CorrelationProperty.Name, context.CorrelationProperty.Value);
        }

        public static Guid Generate(RuntimeSagaInfo sagaInfo, string correlationPropertyName, object correlationPropertyValue)
        {
            var serializedPropertyValue = JsonConvert.SerializeObject(correlationPropertyValue);
            return DeterministicGuid($"{sagaInfo.SagaAttribute.SagaDataName}_{correlationPropertyName}_{serializedPropertyValue}");
        }

        static Guid DeterministicGuid(string src)
        {
            var stringbytes = Encoding.UTF8.GetBytes(src);
            using (var sha1CryptoServiceProvider = new SHA1CryptoServiceProvider())
            {
                var hashedBytes = sha1CryptoServiceProvider.ComputeHash(stringbytes);
                Array.Resize(ref hashedBytes, 16);
                return new Guid(hashedBytes);
            }
        }
    }
}