using System;
using System.Collections.Generic;
using System.Text;

namespace MomentumDiscordBot.Utilities
{
    public static class DateTimeExtensions
    {
        public static string GetTimeStringSinceDateTime(this DateTime dateTime)
        {
            TimeSpan deltaTime;
            if (dateTime.Ticks < DateTime.Now.Ticks)
            {
                deltaTime = DateTime.Now - dateTime;
                return deltaTime.ToPrettyFormat() + " ago";
            }
            else
            {
                deltaTime = dateTime - DateTime.Now;
                return deltaTime.ToPrettyFormat() + " in the future";

            }
        }
        public static string ToPrettyFormat(this TimeSpan span)
        {

            if (span == TimeSpan.Zero) return "0 minutes";

            var sb = new StringBuilder();
            if (span.Days / 365 > 0)
            {
                var approximateYears = span.Days / 365;
                sb = sb.AppendFormat("{0} year{1} ", approximateYears, approximateYears > 1 ? "s" : string.Empty);
            }
            // Modulo 365 because we remove approximately each year above
            if (span.Days % 365 > 0)
                sb = sb.AppendFormat("{0} day{1} ", span.Days % 365, span.Days % 365 > 1 ? "s" : string.Empty);
            if (span.Hours > 0)
                sb = sb.AppendFormat("{0} hour{1} ", span.Hours, span.Hours > 1 ? "s" : string.Empty);
            if (span.Minutes > 0)
                sb = sb.AppendFormat("{0} minute{1} ", span.Minutes, span.Minutes > 1 ? "s" : string.Empty);
            return sb.ToString();

        }
    }
}
