using GreenKeeper.Commands;
using GreenKeeper.Models.Enums;
using GreenKeeper.ViewModels.Base;
using System.Windows.Input;

namespace GreenKeeper.ViewModels.CareStatuses.Abstract
{
    /// <summary>
    /// One status card of the dashboard. StatusCardStyles.xaml picks its icon and color
    /// from the care type.
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

        // The status text as a screen reader should say it; a card with abbreviations writes them out.
        public virtual string SpokenStatusText => StatusText;

        /// <summary>
        /// Whether the status text reads as overdue. It sits on the base class so the shared card
        /// template can bind it on every card; Sunlight has no due date and stays false.
        /// </summary>
        public virtual bool IsOverdue => false;

        /// <summary>
        /// Raises PropertyChanged for everything that depends on the current day. The card
        /// stays, because a screen reader would announce a replaced one again (see
        /// MainViewModel.RefreshDueStatus).
        /// </summary>
        public virtual void NotifyDueStatusChanged()
        {
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(SpokenStatusText));
            OnPropertyChanged(nameof(IsOverdue));
        }
    }
}
