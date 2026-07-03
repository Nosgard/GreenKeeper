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
    public class WateringStatusViewModel : CareStatusViewModel
    {
        public WateringStatusViewModel(CareSchedule? schedule)
            : base(CareType.Water, schedule, "Watering", "/Resources/Icons/Waterdrop.png", "#4accff")
        {

        }
    }
}
