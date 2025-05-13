
namespace Common
{
    public static class RankingSuffix
    {

        public static string GetOrdinalSuffix(int ranking)
        {
            if (ranking <= 0 || ranking > 100)
                throw new ArgumentOutOfRangeException(nameof(ranking), "Ranking must be between 1 and 100.");

            if (ranking % 100 >= 11 && ranking % 100 <= 13)
                return $"{ranking}th";

            return ranking switch
            {
                _ when ranking % 10 == 1 => $"{ranking}st",
                _ when ranking % 10 == 2 => $"{ranking}nd",
                _ when ranking % 10 == 3 => $"{ranking}rd",
                _ => $"{ranking}th",
            };
        }
    }
}
