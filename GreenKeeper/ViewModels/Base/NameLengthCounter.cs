namespace GreenKeeper.ViewModels.Base
{
    /// <summary>
    /// The counter below a plant name field, worded the same in the Add Plant wizard
    /// and in the Rename dialog.
    /// </summary>
    public static class NameLengthCounter
    {
        public static string TextFor(int charactersRemaining)
        {
            return charactersRemaining == 1 ? "1 character remaining" : $"{charactersRemaining} characters remaining";
        }
    }
}
