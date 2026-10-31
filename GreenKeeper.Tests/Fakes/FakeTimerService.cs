using GreenKeeper.Services;

namespace GreenKeeper.Tests.Fakes
{

    /// <summary>
    /// ITimerService without a real timer, so no tick fires during or after a test.
    /// MainViewModel's constructor still calls Start, and the fake keeps the callback:
    /// TriggerTick() runs it as if the interval had passed.
    /// </summary>
    public class FakeTimerService : ITimerService
    {
        private Action? _callback;

        public bool StartWasCalled { get; private set; }
        public bool StopWasCalled { get; private set; }
        public TimeSpan? LastInterval { get; private set; }

        public void Start(TimeSpan interval, Action callback)
        {
            StartWasCalled = true;
            LastInterval = interval;
            _callback = callback;
        }

        public void Stop()
        {
            StopWasCalled = true;
        }

        public void TriggerTick()
        {
            _callback?.Invoke();
        }
    }
}
