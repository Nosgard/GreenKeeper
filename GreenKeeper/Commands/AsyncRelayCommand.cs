using System.Windows.Input;

namespace GreenKeeper.Commands
{
    /// <summary>
    /// RelayCommand for asynchronous work. ICommand.Execute has to be void, so
    /// it awaits ExecuteAsync in the only place an "async void" is unavoidable -
    /// the ViewModels themselves stay on plain Tasks, and tests can await
    /// ExecuteAsync and see its exceptions.
    /// </summary>
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<object?, Task> _execute;
        private readonly Predicate<object?>? _canExecute;

        public AsyncRelayCommand(Func<object?, Task> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute?.Invoke(parameter) ?? true;
        }

        public async void Execute(object? parameter)
        {
            await ExecuteAsync(parameter);
        }

        public Task ExecuteAsync(object? parameter)
        {
            return _execute(parameter);
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
