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
    public class FertilizingStatusViewModel : CareStatusViewModel
    {
        public FertilizingStatusViewModel(CareSchedule? schedule)
            : base(CareType.Nutrients, schedule, "Fertilizing", "/Resources/Icons/Pill.png", "#ff695b")
        {

        }
    }
}
