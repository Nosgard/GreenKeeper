using GreenKeeper.Models.Enums;
using GreenKeeper.ViewModels.Base;
using System.Windows.Input;

namespace GreenKeeper.ViewModels.CareStatuses.Abstract
{
    /// <summary>
    /// One status card of the dashboard. Icon and color of a card are chosen by
    /// the card template from the care type (StatusCardStyles.xaml), so no
    /// UI detail lives here.
    /// </summary>
    public abstract class CareStatusViewModel : ObservableObject
    {
        public CareType Care { get; }
        public string Title => Care.DisplayName();

        // Removes an optional status card (Fertilizing, Sunlight).
        // Nullable because mandatory status cards such as Watering are unremovable, so for them it is null.
        public ICommand? RemoveCommand { get; protected set; }

        // Allows the user to edit the value of a status card that was set beforehand.
        // This applies to all available status cards, but it is set by the concrete ViewModel of the status.
        public ICommand? EditCommand { get; protected set; }

        protected CareStatusViewModel(CareType care)
        {
            Care = care;
        }

        // All status cards implement the status text in their own way.
        public abstract string StatusText { get; }

        /// <summary>
        /// Whether the status text should read as overdue. Sits on the base class
        /// so the shared card template can bind it on every card - Sunlight has no
        /// due date and stays false.
        /// </summary>
        public virtual bool IsOverdue => false;
    }
}
