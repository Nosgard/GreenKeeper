using GreenKeeper.Commands;
using GreenKeeper.Models;
using GreenKeeper.Models.Enums;

namespace GreenKeeper.ViewModels.CareStatuses.Active
{
    /// <summary>
    /// The status card for Watering. DashboardViewModel passes the callbacks for Complete and
    /// Edit; watering is mandatory, so the card has no RemoveCommand.
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
