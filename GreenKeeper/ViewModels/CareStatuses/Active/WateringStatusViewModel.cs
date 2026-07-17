using GreenKeeper.Commands;
using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.ViewModels.CareStatuses.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.ViewModels.CareStatuses.Active
{
    // <summary>
    /// Set the Status-Card for Watering via the given schedule.
    /// onEdit: Will be given by MainViewModel and encapsulates the EditscheduleView for this Care-Type.
    /// The Watering-Status is mandatory so no RemoveCommand is needed
    /// 
    /// Note: The ViewModel neither knows the Plant-Object nor any Window-Class,
    /// it only triggers the given Action
    /// </summary>
    public class WateringStatusViewModel : ActiveCareStatusViewModel
    {
        public WateringStatusViewModel(CareSchedule? schedule, Action onComplete, Action onEdit)
            : base(CareType.Water, schedule, "Watering", "/Resources/Icons/Waterdrop.png", "#4accff")
        {
            CompleteCommand = new RelayCommand(_ => onComplete());

            EditCommand = new RelayCommand(
                _ => onEdit());
        }
    }
}
