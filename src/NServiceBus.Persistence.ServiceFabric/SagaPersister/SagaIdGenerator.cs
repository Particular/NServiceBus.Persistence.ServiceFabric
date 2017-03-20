namespace NServiceBus.Persistence.ServiceFabric
{
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using Newtonsoft.Json;
    using Sagas;

    class SagaIdGenerator : ISagaIdGenerator
    {
        public Guid Generate(SagaIdGeneratorContext context)
        {
            if (context.CorrelationProperty == SagaCorrelationProperty.None)
            {
                return Guid.NewGuid();
            }

            return Generate(context.SagaMetadata.SagaEntityType, context.CorrelationProperty.Name, context.CorrelationProperty.Value);
        }

        public static Guid Generate(Type sagaEntityType, string correlationPropertyName, object correlationPropertyValue)
        {
            var serializedPropertyValue = JsonConvert.SerializeObject(correlationPropertyValue);
            // Needs to be discussed if FullName is a good idea
            return DeterministicGuid($"{sagaEntityType.FullName}_{correlationPropertyName}_{serializedPropertyValue}");
        }

        static Guid DeterministicGuid(string src)
        {
            var stringbytes = Encoding.UTF8.GetBytes(src);
            // TODO; Should we cache this?
            using (var sha1CryptoServiceProvider = new SHA1CryptoServiceProvider())
            {
                var hashedBytes = sha1CryptoServiceProvider.ComputeHash(stringbytes);
                Array.Resize(ref hashedBytes, 16);
                return new Guid(hashedBytes);
            }
        }
    }
}