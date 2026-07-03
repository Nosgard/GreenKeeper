using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.ViewModels.CareStatuses.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.ViewModels.CareStatuses
{
    public class SunlightStatusViewModel : CareStatusViewModel
    {
        public SunlightStatusViewModel(CareSchedule? schedule)
            : base(CareType.Sunlight, schedule, "Sunlight", "/Resources/Icons/Sun.png", "#ffcc00")
        {

        }
    }
}
