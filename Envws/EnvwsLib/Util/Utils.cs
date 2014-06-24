using System;

namespace EnvwsLib.Util
{
    public class Utils
    {
        public static long CurrentUTCMillies()
        {
            return DateTime.UtcNow.Ticks/TimeSpan.TicksPerMillisecond;
        }
    }
}
