using GreenKeeper.Commands;
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

        // Removes an optional card (Fertilizing, Sunlight); null for Watering, which is mandatory.
        public AsyncRelayCommand? RemoveCommand { get; protected set; }

        // Opens the edit dialog of the card; every card has one, set by the subclass.
        public ICommand? EditCommand { get; protected set; }

        protected CareStatusViewModel(CareType care)
        {
            Care = care;
        }

        public abstract string StatusText { get; }

        // The status text the way a screen reader should say it. Most status texts
        // read well as they are; a card with abbreviations writes them out.
        public virtual string SpokenStatusText => StatusText;

        /// <summary>
        /// Whether the status text reads as overdue. It sits on the base class so the shared card
        /// template can bind it on every card; Sunlight has no due date and stays false.
        /// </summary>
        public virtual bool IsOverdue => false;

        /// <summary>
        /// Re-reads everything that depends on the current day without replacing the card.
        /// A new card would make the UI build a new element, which a screen reader
        /// announces again (see MainViewModel.RefreshDueStatus).
        /// </summary>
        public virtual void NotifyDueStatusChanged()
        {
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(SpokenStatusText));
            OnPropertyChanged(nameof(IsOverdue));
        }
    }
}
