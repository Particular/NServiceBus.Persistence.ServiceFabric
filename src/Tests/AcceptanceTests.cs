namespace Tests
{
    using System;

    public class AcceptanceTests : R<AcceptanceTests>
    {
        public static Guid ImageStorePath { get; set; } = new Guid("3DF07CFA-F94D-4927-A46B-28595B720285");
        public static string ApplicationTypeName { get; set; } = "PersistenceTestsType";
        public static Version ApplicationTypeVersion { get; set; } = new Version(1, 0, 0);
        public static Uri ApplicationName { get; set; } = new Uri("fabric:/PersistenceTests");
        public static Uri ServiceUri { get; set; } = new Uri("fabric:/PersistenceTests/AcceptanceTests");
    }
}