using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Huppy.Core.Utilities
{
    public static class Miscellaneous
    {
        public static ulong DateTimeToUnixTimestamp(DateTime dateTime)
        {
            return (ulong)(TimeZoneInfo.ConvertTimeToUtc(dateTime) -
                   new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;
        }
    }
}
