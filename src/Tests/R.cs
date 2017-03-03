﻿namespace Tests
{
    using System;
    using System.Collections.Generic;
    using System.Fabric;
    using System.Fabric.Description;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization.Json;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Microsoft.ServiceFabric.Services.Remoting.Client;
    using NUnit.Framework;
    using NUnit.Framework.Interfaces;
    using TestRunner.Interfaces;

    [TestFixture]
    public abstract class R<TSelf>
    {
        static R()
        {
            var properties = typeof(TSelf).GetProperties(BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public).ToDictionary(p => p.Name, p => p.GetValue(null));
            ImageStorePath = (Guid)properties[nameof(ImageStorePath)];
            ApplicationTypeName = (string)properties[nameof(ApplicationTypeName)];
            ApplicationTypeVersion = (Version)properties[nameof(ApplicationTypeVersion)];
            ApplicationName = (Uri)properties[nameof(ApplicationName)];
            ServiceUri = (Uri)properties[nameof(ServiceUri)];
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
        public async Task ServiceFabricTearDown()
        {
            await TearDown().ConfigureAwait(false);
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
                var clusterManifest = await GetClusterManifest(new Uri("http://localhost:19080")).ConfigureAwait(false);
                imageStoreConnectionString = clusterManifest["Management"]["ImageStoreConnectionString"];

                var directoryName = "Release";
#if DEBUG
                directoryName = "Debug";
#endif

                var testAppPkgPath = Path.Combine(DetermineCallerFilePath(), $@"..\NServiceBus.Persistence.ServiceFabric.Application\pkg\{directoryName}");

                using (var fabric = new FabricClient())
                {
                    var app = fabric.ApplicationManager;
                    await TearDown().ConfigureAwait(false); // TODO we need a more optimal way
                    app.CopyApplicationPackage(imageStoreConnectionString, testAppPkgPath, ImageStorePath.ToString());
                    await app.ProvisionApplicationAsync(ImageStorePath.ToString()).ConfigureAwait(false);
                    await app.CreateApplicationAsync(new ApplicationDescription(ApplicationName, ApplicationTypeName, ApplicationTypeVersion.ToString())).ConfigureAwait(false);
                }

                testRunner = ServiceProxy.Create<ITestRunner>(ServiceUri);
            }
        }

        static async Task<Dictionary<string, Dictionary<string, string>>> GetClusterManifest(Uri clusterUri)
        {
            using (var client = new HttpClient())
            using (var response = await client.GetStreamAsync(new Uri(clusterUri, "/$/GetClusterManifest?api-version=1.0")).ConfigureAwait(false))
            {
                var serializer = new DataContractJsonSerializer(typeof(ClusterManifest));
                var clusterManifest = (ClusterManifest) serializer.ReadObject(response);
                using (var reader = new StringReader(clusterManifest.Manifest))
                {
                    var document = XDocument.Load(reader);
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
            }
        }

        static string DetermineCallerFilePath([CallerFilePath] string path = null)
        {
            return Path.GetDirectoryName(path);
        }

        static Guid ImageStorePath;
        static string ApplicationTypeName;
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