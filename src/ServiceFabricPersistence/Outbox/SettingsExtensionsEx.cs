namespace NServiceBus.Persistence.ServiceFabric
{
    using System;
    using Features;
    using Settings;

    static class SettingsExtensionsEx
    {
        internal static void EnableFeature(this SettingsHolder settings, Type featureType)
        {
            settings.Set(featureType.FullName, FeatureState.Enabled);
        }
    }
}