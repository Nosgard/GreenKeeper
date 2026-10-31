using GreenKeeper.Commands;
using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.ViewModels.CareStatuses.Abstract;

namespace GreenKeeper.ViewModels.CareStatuses.Passive
{
    /// <summary>
    /// The status card for Sunlight, a passive care without a due date. DashboardViewModel
    /// passes the callbacks for Edit and Remove.
    /// </summary>
    public class SunlightStatusViewModel : CareStatusViewModel
    {
        private readonly SunlightRequirement _sunlightRequirement;

        public SunlightStatusViewModel(SunlightRequirement sunlightRequirement, Action onEdit, Func<Task> onRemove)
            : base(CareType.Sunlight)
        {
            _sunlightRequirement = sunlightRequirement;

            EditCommand = new RelayCommand(_ => onEdit());
            RemoveCommand = new AsyncRelayCommand(_ => onRemove());
        }

        public override string StatusText => $"{_sunlightRequirement.Hours}h / {_sunlightRequirement.Period.NameInText()}";

        public override string SpokenStatusText => _sunlightRequirement.Period.SpokenHours(_sunlightRequirement.Hours);
    }
}
