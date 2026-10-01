namespace GreenKeeper.Models
{
    public class Plant
    {
        // One limit for the Add Plant wizard, the rename dialog and their input fields.
        public const int MaxNameLength = 50;

        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImagePath { get; set; }
        public string? Notes { get; set; }

        // Navigation properties
        public List<CareSchedule> CareSchedules { get; } = new();
        public SunlightRequirement? SunlightRequirement { get; set; }

        // A name consisting only of spaces would look empty in the sidebar
        // but still pass a simple null/empty check.
        public static bool IsValidName(string? name)
        {
            return !string.IsNullOrWhiteSpace(name) && name.Length <= MaxNameLength;
        }
    }
}
