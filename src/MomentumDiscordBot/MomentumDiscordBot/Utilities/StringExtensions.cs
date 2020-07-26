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
    }
}