using GreenKeeper.Commands;
using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.ViewModels.CareStatuses.Abstract;

namespace GreenKeeper.ViewModels.CareStatuses.Passive
{
    /// <summary>
    /// Set the status card for Sunlight via the related sunlight requirement (passive care status).
    /// onEdit / onRemove: Are provided by MainViewModel and encapsulate the EditScheduleView
    /// for this care type and the confirmation + removal.
    ///
    /// Note: The ViewModel knows neither the plant object nor any window class;
    /// it only triggers the given callbacks.
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

        public override string StatusText
        {
            get
            {
                string periodText = _sunlightRequirement.Period switch
                {
                    SunlightPeriod.Day => "day",
                    SunlightPeriod.Week => "week",
                    SunlightPeriod.Month => "month",
                    SunlightPeriod.Year => "year",
                    _ => string.Empty
                };

                return $"{_sunlightRequirement.Hours}h / {periodText}";
            }
        }
    }
}
