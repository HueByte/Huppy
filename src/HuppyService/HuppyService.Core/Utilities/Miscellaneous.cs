using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuppyService.Core.Utilities
{
    public static class Miscellaneous
    {
        public static DateTime UnixTimeStampToDateTime(ulong unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }

        public static ulong DateTimeToUnixTimeStamp(DateTime date)
        {
            return (ulong)(TimeZoneInfo.ConvertTimeToUtc(date) -
                   new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;
        }
    }
}
