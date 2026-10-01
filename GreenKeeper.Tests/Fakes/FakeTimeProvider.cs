namespace GreenKeeper.Tests.Fakes
{
    /// <summary>
    /// A clock the test sets, so due dates and "today" are the same on every run
    /// and every day of the year. The value is treated as local time, like DateTime.Now.
    /// </summary>
    public class FakeTimeProvider : TimeProvider
    {
        public DateTime Now { get; set; }

        public FakeTimeProvider(DateTime now)
        {
            Now = now;
        }

        public override DateTimeOffset GetUtcNow()
        {
            return new DateTimeOffset(DateTime.SpecifyKind(Now, DateTimeKind.Local)).ToUniversalTime();
        }

        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Local;
    }
}
