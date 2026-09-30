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
    /// <summary>
    /// Set the status card for Watering via the given schedule.
    /// onEdit: Is provided by MainViewModel and encapsulates the EditScheduleView for this care type.
    /// The watering status is mandatory, so no RemoveCommand is needed.
    /// 
    /// Note: The ViewModel knows neither the plant object nor any window class;
    /// it only triggers the given Action.
    /// </summary>
    public class WateringStatusViewModel : ActiveCareStatusViewModel
    {
        public WateringStatusViewModel(CareSchedule? schedule, Action onComplete, Action onEdit)
            : base(CareType.Watering, schedule, "Watering", "/Resources/Icons/Waterdrop.png", "#4accff")
        {
            CompleteCommand = new RelayCommand(_ => onComplete());

            EditCommand = new RelayCommand(
                _ => onEdit());
        }
    }
}
