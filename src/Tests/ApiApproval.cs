using System.IO;
using System.Runtime.CompilerServices;
using ApiApprover;
using NServiceBus.Persistence.ServiceFabric;
using NUnit.Framework;

[TestFixture]
public class ApiApproval
{
    [Test]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public void Approve()
    {
        Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
        PublicApiApprover.ApprovePublicApi(typeof(ServiceFabricPersistence).Assembly);
    }
}