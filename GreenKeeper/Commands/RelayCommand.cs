using System.Windows.Input;

namespace GreenKeeper.Commands
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute?.Invoke(parameter) ?? true;
        }

        public void Execute(object? parameter)
        {
            _execute(parameter);
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <summary>
        /// Has every command check again whether it can run. Buttons otherwise update only
        /// after the user releases a key or mouse button, or after the focus moves, so call it
        /// after a state change like a wizard step switch.
        /// The check runs later, so a button still shows its old state right after this call.
        /// </summary>
        public static void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
