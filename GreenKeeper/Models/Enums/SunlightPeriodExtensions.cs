namespace GreenKeeper.Models.Enums
{
    public static class SunlightPeriodExtensions
    {
        // The period as a word in running text, not capitalized ("6h / day").
        public static string NameInText(this SunlightPeriod period)
        {
            return period switch
            {
                SunlightPeriod.Day => "day",
                SunlightPeriod.Week => "week",
                SunlightPeriod.Month => "month",
                SunlightPeriod.Year => "year",
                _ => string.Empty
            };
        }

        // Hours of sunlight the way a screen reader should say them: the card's "6h / day"
        // would be read as "6 h slash day", the summary's "6 Hours / Day" as "6 Hours slash Day".
        public static string SpokenHours(this SunlightPeriod period, int hours)
        {
            return $"{hours} {(hours == 1 ? "hour" : "hours")} per {period.NameInText()}";
        }
    }
}
