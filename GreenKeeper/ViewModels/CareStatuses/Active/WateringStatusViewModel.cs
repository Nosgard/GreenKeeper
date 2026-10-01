using GreenKeeper.Commands;
using GreenKeeper.Models;
using GreenKeeper.Models.Enums;

namespace GreenKeeper.ViewModels.CareStatuses.Active
{
    /// <summary>
    /// Set the status card for Watering via the given schedule.
    /// onComplete / onEdit: Are provided by MainViewModel and encapsulate the completion
    /// and the EditScheduleView for this care type.
    /// The watering status is mandatory, so no RemoveCommand is needed.
    ///
    /// Note: The ViewModel knows neither the plant object nor any window class;
    /// it only triggers the given callbacks.
    /// </summary>
    public class WateringStatusViewModel : ActiveCareStatusViewModel
    {
        public WateringStatusViewModel(CareSchedule? schedule, Func<Task> onComplete, Action onEdit, TimeProvider timeProvider)
            : base(CareType.Watering, schedule, timeProvider)
        {
            CompleteCommand = new AsyncRelayCommand(_ => onComplete());
            EditCommand = new RelayCommand(_ => onEdit());
        }
    }
}
