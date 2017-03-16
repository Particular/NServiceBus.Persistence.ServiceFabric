namespace NServiceBus.Persistence.ServiceFabric
{
    using System;

    static class Guard
    {
        public static void AgainstNegativeAndZero(string argumentName, TimeSpan value)
        {
            if (value <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(argumentName);
        }
    }
}