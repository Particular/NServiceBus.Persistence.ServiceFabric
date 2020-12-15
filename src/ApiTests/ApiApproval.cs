using NServiceBus.Persistence.ServiceFabric;
using NUnit.Framework;
using Particular.Approvals;
using PublicApiGenerator;

[TestFixture]
public class APIApprovals
{
    [Test]
    public void ApproveServiceFabricPersistence()
    {
        var publicApi = ApiGenerator.GeneratePublicApi(typeof(ServiceFabricPersistence).Assembly, excludeAttributes: new[] { "System.Reflection.AssemblyMetadataAttribute" });
        Approver.Verify(publicApi);
    }
}