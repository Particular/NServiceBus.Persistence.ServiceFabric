namespace Tests
{
    using System;

    public class Tests : R<Tests>
    {
        public static Guid ImageStorePath { get; set; } = new Guid("2DA66BEE-2747-4185-9E5F-3A4951EA074A");
        public static string ApplicationTypeName { get; set; } = "PersistenceTestsType";
        public static Version ApplicationTypeVersion { get; set; } = new Version(1, 0, 0);
        public static Uri ApplicationName { get; set; } = new Uri("fabric:/PersistenceTests");
        public static Uri ServiceUri { get; set; } = new Uri("fabric:/PersistenceTests/Tests");
    }
}