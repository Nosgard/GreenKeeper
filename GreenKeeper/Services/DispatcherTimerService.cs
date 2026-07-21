using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace GreenKeeper.Services
{
    /// <summary>
    /// Implementation of ITimerService, based on DispatcherTimer.
    /// It uses a View-technology and should never be referenced directly
    /// by a ViewModel, only through ITimerService.
    /// 
    /// DispatcherTimer specifically runs its Tick-event on the UI thread
    /// by default, which is why no manual thread-marshalling is needed
    /// when the callback later raises PropertyChanged
    /// </summary>
    public class DispatcherTimerService : ITimerService
    {
        private readonly DispatcherTimer _timer = new();

        // Stores the callback passed into Start(), so OnTick can invoke it
        // whenever the timer fires. Nullable since no callback exists before Start() has been called first
        private Action? _callback;

        public void Start(TimeSpan interval, Action callback)
        {
            _callback = callback;
            _timer.Interval = interval;

            // Unsubscribe first, in case Start() is ever called more than once on the same instance.
            // Without this, OnTick would end up subscribed multiple times
            _timer.Tick -= OnTick;
            _timer.Tick += OnTick;

            _timer.Start();
        }

        // Wired up to DispatcherTimer.Tick. Simply forwards the tick to
        // whatever callback was registered via Start()
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
