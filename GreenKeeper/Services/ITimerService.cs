using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.Services
{
    /// <summary>
    /// Abstraction over a periodic timer. Analogous to IDialogService.
    /// Keeps the ViewModels free of WPF-specific classes like DispatcherTimer, which in turn
    /// keeps them testable
    /// </summary>
    public interface ITimerService
    {
        /// <summary>
        /// Starts invoking the given callback repeatedly at the given interval,
        /// until Stop() is called
        /// </summary>
        /// <param name="interval">How often the callback should be invoked</param>
        /// <param name="callback">The action to invoke on every tick</param>
        void Start(TimeSpan interval, Action callback);

        // Stops any currently running periodic invocation started via Start()
        void Stop();
    }
}
