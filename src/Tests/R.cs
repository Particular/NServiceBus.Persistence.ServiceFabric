﻿namespace Tests
{
    using System;
    using System.Collections.Generic;
    using System.Fabric;
    using System.Fabric.Description;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Microsoft.ServiceFabric.Services.Remoting.Client;
    using NUnit.Framework;
    using NUnit.Framework.Interfaces;
    using TestRunner.Interfaces;

    /// <summary>
    /// This class has a short name on purpose to not clutter the Tree node in VS when the tests are run.
    /// Applications are detected by convention or all the parameters have to be set manually.
    /// Usage:
    /// <code>
    /// <![CDATA[
    /// public class SomeTests : R<SomeTests> { }
    /// ]]>
    /// </code>
    /// The above code will derive by convention the following properties:
    /// - ImageStorePath: Deterministic Guid from SomeTests
    /// - ApplicationTypeName: SomeTestsType
    /// - ApplicationTypeVersion: 1.0.0
    /// - ApplicationName: fabric:/SomeTests
    /// - ServiceUri: fabric:/SomeTests/Tests
    /// - TestAppPkgPath: ..\*.SomeTestsApplication
    /// If the convention needs to be bypassed set it manually with:
    /// <code>
    /// <![CDATA[
    /// public class SomeTests : R<SomeTests> {
    ///             public static Guid ImageStorePath { get; set; } = new Guid("2DA66BEE-2747-4185-9E5F-3A4951EA074A");
    ///             public static string ProjectName { get; set; } = "NServiceBus.Persistence.ServiceFabric.SomeTestsApplication";
    ///             public static string ApplicationTypeName { get; set; } = "SomeTestsType";
    ///             public static Version ApplicationTypeVersion { get; set; } = new Version(1, 0, 0);
    ///             public static Uri ApplicationName { get; set; } = new Uri("fabric:/SomeTests");
    ///             public static Uri ServiceUri { get; set; } = new Uri("fabric:/SomeTests/Tests");
    /// }
    /// ]]>
    /// </code>
    /// </summary>
    [TestFixture]
    public abstract class R<TSelf>
    {
        static R()
        {
            var properties = typeof(TSelf).GetProperties(BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public).ToDictionary(p => p.Name, p => p.GetValue(null));

            // apply conventions
            if (properties.Count == 0)
            {
                var typeName = typeof(TSelf).Name;
                ImageStorePath = DeterministicGuid(typeName);
                ApplicationTypeName = $"{typeName}Type";
                ApplicationTypeVersion = new Version(1, 0, 0);
                ApplicationName = new Uri($"fabric:/{typeName}");
                ServiceUri = new Uri($"fabric:/{typeName}/Tests");
                var oneLevelUp = Path.Combine(DetermineCallerFilePath(), @"..\");
                var applicationName = $"{typeName}Application";
                var directory = Directory.EnumerateDirectories(oneLevelUp, $"*.{applicationName}", SearchOption.TopDirectoryOnly).Single();
#if DEBUG
                var directoryName = "Debug";
#else
                var directoryName = "Release";
#endif

                TestAppPkgPath = $@"{directory}\pkg\{directoryName}";
            }
            // grab or throw
            else
            {
                ImageStorePath = (Guid)properties[nameof(ImageStorePath)];
                ApplicationTypeName = (string)properties[nameof(ApplicationTypeName)];
                TestAppPkgPath = (string)properties[nameof(TestAppPkgPath)];
                ApplicationTypeVersion = (Version)properties[nameof(ApplicationTypeVersion)];
                ApplicationName = (Uri)properties[nameof(ApplicationName)];
                ServiceUri = (Uri)properties[nameof(ServiceUri)];
            }
        }

        [Timeout(600000)]
        [TestCaseSource(nameof(GetTestCases))]
        public async Task _(string testName)
        {
            var result = await testRunner.Run(testName).ConfigureAwait(false);
            if (result.HasOutput)
            {
                Console.WriteLine(result.Output);
            }

            if (result.HasException)
            {
                throw result.Exception;
            }
        }

        [OneTimeTearDown]
        public Task ServiceFabricTearDown()
        {
            return TearDown();
        }

        static async Task TearDown()
        {
            using (var fabric = new FabricClient())
            {
                var app = fabric.ApplicationManager;
                var applications = await fabric.QueryManager.GetApplicationListAsync(ApplicationName).ConfigureAwait(false);
                if (applications.Any())
                {
                    await app.DeleteApplicationAsync(new DeleteApplicationDescription(ApplicationName)).ConfigureAwait(false);
                    await app.UnprovisionApplicationAsync(ApplicationTypeName, ApplicationTypeVersion.ToString()).ConfigureAwait(false);
                    app.RemoveApplicationPackage(imageStoreConnectionString, ImageStorePath.ToString());
                }
            }
        }

        public static IEnumerable<ITestCaseData> GetTestCases()
        {
            SetUp().GetAwaiter().GetResult();

            foreach (var test in testRunner.Tests().GetAwaiter().GetResult())
            {
                var testCaseData = new TestCaseData(test)
                {
                    TestName = test
                };
                yield return testCaseData;
            }
        }

        static async Task SetUp()
        {
            if (testRunner == null)
            {
                using (var fabric = new FabricClient())
                {
                    var clusterManifest = await GetClusterManifest(fabric).ConfigureAwait(false);
                    imageStoreConnectionString = clusterManifest["Management"]["ImageStoreConnectionString"];
                    await TearDown().ConfigureAwait(false);
                    var app = fabric.ApplicationManager;
                    app.CopyApplicationPackage(imageStoreConnectionString, TestAppPkgPath, ImageStorePath.ToString());
                    await app.ProvisionApplicationAsync(ImageStorePath.ToString()).ConfigureAwait(false);
                    await app.CreateApplicationAsync(new ApplicationDescription(ApplicationName, ApplicationTypeName, ApplicationTypeVersion.ToString())).ConfigureAwait(false);
                }

                testRunner = ServiceProxy.Create<ITestRunner>(ServiceUri);
            }
        }

        static async Task<Dictionary<string, Dictionary<string, string>>> GetClusterManifest(FabricClient fabricClient)
        {
            var rawManifest = await fabricClient.ClusterManager.GetClusterManifestAsync().ConfigureAwait(false);
            var document = XDocument.Parse(rawManifest);
            var ns = document.Root.GetDefaultNamespace();
            var sections = new Dictionary<string, Dictionary<string, string>>();
            foreach (var section in document.Descendants(ns + "Section"))
            {
                var dictionary = new Dictionary<string, string>();

                foreach (var parameter in section.Descendants(ns + "Parameter"))
                {
                    dictionary.Add(parameter.Attribute("Name").Value, parameter.Attribute("Value").Value);
                }

                sections.Add(section.Attribute("Name").Value, dictionary);
            }
            return sections;
        }

        static string DetermineCallerFilePath([CallerFilePath] string path = null)
        {
            return Path.GetDirectoryName(path);
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

        static Guid ImageStorePath;

        static string ApplicationTypeName;
        static string TestAppPkgPath;
        static Version ApplicationTypeVersion;
        static Uri ApplicationName;
        static Uri ServiceUri;

        static string imageStoreConnectionString;
        static ITestRunner testRunner;

        // ReSharper disable once MemberCanBePrivate.Global
        public class ClusterManifest
        {
            public string Manifest { get; set; }
        }
    }
}