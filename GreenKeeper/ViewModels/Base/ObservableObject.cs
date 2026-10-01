using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GreenKeeper.ViewModels.Base
{
    /// <summary>
    /// Shared INotifyPropertyChanged implementation for every ViewModel, so the
    /// event and the "compare, assign, notify" setter pattern live in one place.
    /// </summary>
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        // The caller's member name is filled in by the compiler, so a setter can
        // simply call OnPropertyChanged() for its own property.
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Assigns the value and notifies, but only if it actually changed.
        /// Returns whether it did, so a setter can notify dependent properties
        /// in that case only.
        /// </summary>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
