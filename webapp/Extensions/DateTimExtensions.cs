using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace CRM.Web.Extensions
{
    public static class DateTimExtensions
    {
        public static int GetWeek(this DateTime time)
        {
            //GetIso8601WeekOfYear

            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            // Return the week of our adjusted day
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }
        public static DateTime GetStartOfWeekDate(this DateTime dt, DayOfWeek firstDay)
        {
            //if the need for multiple cultures arise call this with CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek
            int diff = (7 + (dt.DayOfWeek - firstDay)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
    }
}