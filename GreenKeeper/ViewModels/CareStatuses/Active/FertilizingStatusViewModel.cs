using GreenKeeper.Commands;
using GreenKeeper.Models;
using GreenKeeper.Models.Enums;

namespace GreenKeeper.ViewModels.CareStatuses.Active
{
    /// <summary>
    /// The status card for Fertilizing. DashboardViewModel passes the callbacks for Complete,
    /// Edit and Remove.
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
