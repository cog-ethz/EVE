using System;
using System.Collections.Generic;

namespace EVE.Scripts.Utils
{
    public static class TimeUtils
    {
        
        /// <summary>
        /// The difference between two timestamps
        /// </summary>
        /// <param name="time1">First timestamp.</param>
        /// <param name="time2">Second timestamp.</param>
        /// <returns>The difference.</returns>
        public static float TimeDifference(string time1, string time2)
        {
            return (float) (Convert.ToDateTime(time2)-Convert.ToDateTime(time1)).TotalMilliseconds * 1000;
        }

        /// <summary>
        /// The time span between two timestamps.
        /// </summary>
        /// <param name="time1">First timestamp.</param>
        /// <param name="time2">Second timestamp.</param>
        /// <returns>The time span.</returns>
        public static TimeSpan TimeSpanDifference(string time1, string time2)
        {
            return Convert.ToDateTime(time2) - Convert.ToDateTime(time1);
        }

        /// <summary>
        /// The current system time formatted for the DB.
        /// </summary>
        /// <returns>Formatted timestamp.</returns>
        public static string GetDbTimeStamp()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Formatting a timestamp for the DB.
        /// </summary>
        /// <param name="time">Timestamp to be formatted.</param>
        /// <returns>Formatted timestamp.</returns>
        public static string GetTimeStamp(DateTime time)
        {
            return time.ToString("yyyy-MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
        }
        
        /// <summary>
        /// Sorts a list according to timestamps.
        /// </summary>
        /// <param name="list">List to be sorted.</param>
        /// <returns>Sorted list.</returns>
        public static List<DateTime> SortDatesAscending(List<DateTime> list)
        {
            list.Sort((a, b) => a.CompareTo(b));
            return list;
        }
    }
}
