namespace GreenKeeper.Models.Enums
{
    public static class CareTypeExtensions
    {
        // The name shown on status cards, in dialog titles and wizard steps.
        public static string DisplayName(this CareType careType)
        {
            return careType switch
            {
                CareType.Watering => "Watering",
                CareType.Fertilizing => "Fertilizing",
                CareType.Sunlight => "Sunlight",
                _ => throw new ArgumentOutOfRangeException(nameof(careType), careType, null)
            };
        }

        // The schedule of that care type in the middle of a warning, where it is
        // not capitalized ("Are you sure you want to remove the fertilizing schedule").
        public static string ScheduleNameInSentence(this CareType careType)
        {
            return $"{careType.DisplayName().ToLowerInvariant()} schedule";
        }
    }
}
