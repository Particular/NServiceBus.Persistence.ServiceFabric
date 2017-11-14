namespace NServiceBus.Persistence.ServiceFabric
{
    using Settings;

    public partial class SagaSettings
    {

        SettingsHolder settings;

        internal SagaSettings(SettingsHolder settings)
        {
            this.settings = settings;
        }

    }
}