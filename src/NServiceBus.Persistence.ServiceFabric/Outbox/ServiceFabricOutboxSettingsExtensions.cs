namespace NServiceBus
{
    using System;
    using Configuration.AdvanceExtensibility;
    using Outbox;
    using Persistence.ServiceFabric;

    /// <summary>
    /// Contains InMemoryOutbox related settings extensions.
    /// </summary>
    public static class ServiceFabricOutboxSettingsExtensions
    {
        /// <summary>
        /// Sets the time to keep the deduplication data to the specified time span.
        /// </summary>
        /// <param name="configuration">The configuration being extended</param>
        /// <param name="timeToKeepDeduplicationData">The time to keep the deduplication data.
        /// The cleanup process removes entries older than the specified time to keep deduplication data, therefore the time span cannot be negative</param>
        /// <returns>The configuration</returns>
        public static OutboxSettings SetTimeToKeepDeduplicationData(this OutboxSettings configuration, TimeSpan timeToKeepDeduplicationData)
        {
            var now = DateTime.UtcNow;
            if (now - timeToKeepDeduplicationData >= now)
            {
                throw new ArgumentException("Specify a non-negative TimeSpan. The cleanup process removes entries older than the specified time to keep deduplication data, therefore the time span cannot be negative.", nameof(timeToKeepDeduplicationData));
            }

            configuration.GetSettings().Set(OutboxPersistenceFeature.TimeToKeepDeduplicationEntries, timeToKeepDeduplicationData);
            return configuration;
        }

        /// <summary>
        /// Sets the frequency to run the deduplication data cleanup task.
        /// </summary>
        /// <param name="configuration">The configuration being extended</param>
        /// <param name="frequencyToRunDeduplicationDataCleanup">The frequency to run the deduplication data cleanup task. By specifying a negative time span (-1) the cleanup task will never run.</param>
        /// <returns>The configuration</returns>
        public static OutboxSettings SetFrequencyToRunDeduplicationDataCleanup(this OutboxSettings configuration, TimeSpan frequencyToRunDeduplicationDataCleanup)
        {
            configuration.GetSettings().Set(OutboxPersistenceFeature.FrequencyToRunDeduplicationDataCleanup, frequencyToRunDeduplicationDataCleanup);
            return configuration;
        }
    }
}