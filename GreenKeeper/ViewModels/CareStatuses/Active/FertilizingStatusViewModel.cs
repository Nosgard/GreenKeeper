using GreenKeeper.Commands;
using GreenKeeper.Models;
using GreenKeeper.Models.Enums;

namespace GreenKeeper.ViewModels.CareStatuses.Active
{
    /// <summary>
    /// Set the status card for Fertilizing via the given schedule.
    /// onComplete / onEdit / onRemove: Are provided by MainViewModel and encapsulate the
    /// completion, the EditScheduleView for this care type and the confirmation + removal.
    ///
    /// Note: The ViewModel knows neither the plant object nor any window class;
    /// it only triggers the given callbacks.
    /// </summary>
    public class FertilizingStatusViewModel : ActiveCareStatusViewModel
    {
        public FertilizingStatusViewModel(
            CareSchedule? schedule, Func<Task> onComplete, Action onEdit, Func<Task> onRemove, TimeProvider timeProvider)
            : base(CareType.Fertilizing, schedule, timeProvider)
        {
            CompleteCommand = new AsyncRelayCommand(_ => onComplete());
            EditCommand = new RelayCommand(_ => onEdit());
            RemoveCommand = new AsyncRelayCommand(_ => onRemove());
        }
    }
}
