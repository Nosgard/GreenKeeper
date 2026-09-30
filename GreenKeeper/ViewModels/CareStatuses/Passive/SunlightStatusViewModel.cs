using GreenKeeper.Commands;
using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.ViewModels.CareStatuses.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.ViewModels.CareStatuses.Passive
{
    public class SunlightStatusViewModel : CareStatusViewModel
    {
        private readonly SunlightRequirement? _sunlightRequirement;

        /// <summary>
        /// Set the status card for Sunlight via the related sunlight requirement (passive care status).
        /// onEdit: Is provided by MainViewModel and encapsulates the EditScheduleView for this care type
        /// onRemove: Is provided by MainViewModel and encapsulates the confirmation + removal.
        /// 
        /// Note: The ViewModel knows neither the plant object nor any window class;
        /// it only triggers the given Action.
        /// </summary>
        public SunlightStatusViewModel(SunlightRequirement sunlightRequirement, Action onEdit, Action onRemove)
            : base(CareType.Sunlight, "Sunlight", "/Resources/Icons/Sun.png", "#ffcc00")
        {
            _sunlightRequirement = sunlightRequirement;

            EditCommand = new RelayCommand(
                _ => onEdit());

            RemoveCommand = new RelayCommand(
                _ => onRemove());
        }

        public override string StatusText
        {
            get
            {
                if (_sunlightRequirement == null)
                {
                    return string.Empty;
                }

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
