using NServiceBus.Persistence.ServiceFabric;
using NUnit.Framework;
using Particular.Approvals;
using PublicApiGenerator;

[TestFixture]
public class ApiApproval
{
    [Test]
    public void Approve()
    {
        var publicApi = ApiGenerator.GeneratePublicApi(typeof(ServiceFabricPersistence).Assembly);
        Approver.Verify(publicApi);
    }
}