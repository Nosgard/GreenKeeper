using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.ViewModels.CareStatuses.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.ViewModels.CareStatuses
{
    public class SunlightStatusViewModel : CareStatusViewModel
    {
        private readonly SunlightRequirement? _sunlightRequirement;

        public SunlightStatusViewModel(SunlightRequirement sunlightRequirement)
            : base(CareType.Sunlight, "Sunlight", "/Resources/Icons/Sun.png", "#ffcc00")
        {
            _sunlightRequirement = sunlightRequirement;
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
