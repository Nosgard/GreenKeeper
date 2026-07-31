using GreenKeeper.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.Tests.Fakes
{

    /// <summary>
    /// Fake implementation of ITimerService for tests. Deliberately does NOT
    /// start a real timer - MainViewModel's constructor calls Start(...) to
    /// begin periodic Status-Card refreshes, but in tests we don't want an
    /// actual background timer running (it would keep firing after the test
    /// finishes, could interfere with other tests, abd serves no purpose here
    /// since tests don't wait multiple minutes for a real tick).
    /// 
    /// The callback is still captured, so a test COULD manually invoke it
    /// later via TriggerTick() to simulate "time has passed and a refresh
    /// happened", without needing to wait for a real interval
    /// </summary>
    public class FakeTimerService : ITimerService
    {
        private Action? _callback;

        public bool StartWasCalled { get; private set; }
        public TimeSpan? LastInterval { get; private set; }

        public void Start(TimeSpan interval, Action callback)
        {
            StartWasCalled = true;
            LastInterval = interval;
            _callback = callback;
        }

        public void Stop()
        {

        }

        // Allows a test to manually simulate a timer tick, without waiting for
        // a real interval elapse
        public void TriggerTick()
        {
            _callback?.Invoke();
        }
    }
}
