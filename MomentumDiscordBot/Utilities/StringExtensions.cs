using System.Linq;

namespace MomentumDiscordBot.Utilities
{
    public static class StringExtensions
    {
        public static int GetNthIndex(this string input, char searchInput, int nOccurrence)
        {
            var count = 0;
            for (var i = 0; i < input.Length; i++)
            {
                if (input[i] != searchInput)
                {
                    continue;
                }

                count++;
                if (count == nOccurrence)
                {
                    return i;
                }
            }

            return -1;
        }

        public static string RemoveControlChars(this string input)
            => new(input.Where(c => !char.IsControl(c) && c != '\u1652' && c != 'ٴ').ToArray());
    }
}