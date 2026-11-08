namespace GreenKeeper.Models
{
    /// <summary>
    /// Where the main window was when it was closed, so it can open there again.
    /// Position and size are those of the normal state, the ones a maximized window
    /// returns to.
    /// </summary>
    public record WindowPlacement
    {
        public double Left { get; init; }
        public double Top { get; init; }
        public double Width { get; init; }
        public double Height { get; init; }
        public bool IsMaximized { get; init; }
    }
}
