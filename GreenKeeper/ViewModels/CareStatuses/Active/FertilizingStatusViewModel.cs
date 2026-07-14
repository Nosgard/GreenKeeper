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
    public class FertilizingStatusViewModel : ActiveCareStatusViewModel
    {
        /// <summary>
        /// Set the Status-Card for Fertilizing via the given schedule.
        /// onRemove: Will be given by MainViewModel and encapsulates the Confirmation + Removement.
        /// 
        /// Note: The ViewModel neither knows the Plant-Object nor the IDialogService, it only triggers Action
        /// </summary>
        /// <param name="schedule"></param>
        /// <param name="onRemove"></param>
        public FertilizingStatusViewModel(CareSchedule? schedule, Action onRemove)
            : base(CareType.Nutrients, schedule, "Fertilizing", "/Resources/Icons/Pill.png", "#ff695b")
        {
            RemoveCommand = new RelayCommand(
                _ => onRemove());
        }
    }
}
