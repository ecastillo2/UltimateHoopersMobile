namespace Common
{
    public static class RankingValue
    {
        // Function to convert ranking string (e.g., "1st", "2nd", "3rd") to integer value
        public static int GetRankingValue(string ranking)
        {
            if (string.IsNullOrEmpty(ranking)) return int.MaxValue; // Handle any empty/invalid rankings gracefully
                                                                    // Remove "st", "nd", "rd", "th" from the ranking string
            var numericRank = new string(ranking.Where(char.IsDigit).ToArray());
            return int.TryParse(numericRank, out var result) ? result : int.MaxValue;
        }
    }
}
