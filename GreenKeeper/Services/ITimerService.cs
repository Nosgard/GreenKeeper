namespace GreenKeeper.Services
{
    /// <summary>
    /// A periodic timer for the ViewModels. They do not use DispatcherTimer directly, so the
    /// tests can use a fake that ticks on demand.
    /// </summary>
    public interface ITimerService
    {
        /// <summary>
        /// Calls the callback once per interval until Stop runs.
        /// </summary>
        /// <param name="interval">Time between two calls</param>
        /// <param name="callback">Runs on every tick</param>
        void Start(TimeSpan interval, Action callback);

        void Stop();
    }
}
