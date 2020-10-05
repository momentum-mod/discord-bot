using System;
using System.Collections.Generic;
using System.Linq;
using DSharpPlus.CommandsNext.Attributes;
using MomentumDiscordBot.Commands.Checks;

namespace MomentumDiscordBot.Utilities
{
    public static class FailedChecksExtensions
    {
        private const string ReasonPrefix = " • ";

        public static string ToCleanResponse(this IEnumerable<CheckBaseAttribute> failedChecks)
        {
            var reasons = failedChecks.Select(x => x.ToCleanReason());

            return ReasonPrefix + string.Join(Environment.NewLine + ReasonPrefix, reasons);
        }

        private static string ToCleanReason(this CheckBaseAttribute check)
        {
            if (check is DescriptiveCheckBaseAttribute descriptiveCheck)
            {
                return descriptiveCheck.FailureResponse;
            }

            return check.ToString();
        }
    }
}