using System.Windows.Threading;

namespace GreenKeeper.Services
{
    /// <summary>
    /// ITimerService on a DispatcherTimer. Its tick runs on the same thread as the windows,
    /// so the callback can change the ViewModels directly, as a button command does.
    /// </summary>
    public class DispatcherTimerService : ITimerService
    {
        private readonly DispatcherTimer _timer = new();

        private Action? _callback;

        public void Start(TimeSpan interval, Action callback)
        {
            _callback = callback;
            _timer.Interval = interval;

            // Start may run more than once; unsubscribing first keeps OnTick attached once.
            _timer.Tick -= OnTick;
            _timer.Tick += OnTick;

            _timer.Start();
        }

        private void OnTick(object? sender, EventArgs e)
        {
            _callback?.Invoke();
        }

        public void Stop()
        {
            _timer.Stop();
        }
    }
}
