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
            if (span.TotalMilliseconds < 1) return "instantaneously ";

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
            if (span.Seconds > 0)
                sb = sb.AppendFormat("{0} second{1} ", span.Seconds, span.Seconds > 1 ? "s" : string.Empty);
            if (span.TotalSeconds < 1 && span.Milliseconds > 0)
                sb = sb.AppendFormat("{0} millisecond{1} ", span.Milliseconds, span.Milliseconds > 1 ? "s" : string.Empty);

            var output = sb.ToString();

            // When years are taken, no need to show ms - use 3 levels of accuracy
            // 3 levels of accuracy * 2 spaces per level
            var thirdSpace = output.GetNthIndex(' ', 3 * 2);

            if (thirdSpace == -1)
            {
                // Less than 3 levels of accuracy, just return the last part
                thirdSpace = output.Length;
            }

            return output.Substring(0, thirdSpace);
        }
    }
}
