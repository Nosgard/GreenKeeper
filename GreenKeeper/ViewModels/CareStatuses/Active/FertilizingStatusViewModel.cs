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
        /// onEdit: Will be given by MainViewModel and encapsulates the EditscheduleView for this Care-Type
        /// onRemove: Will be given by MainViewModel and encapsulates the Confirmation + Removement.
        /// 
        /// Note: The ViewModel neither knows the Plant-Object nor any Window-Class,
        /// it only triggers the given Action
        /// </summary>
        public FertilizingStatusViewModel(CareSchedule? schedule, Action onEdit, Action onRemove)
            : base(CareType.Nutrients, schedule, "Fertilizing", "/Resources/Icons/Pill.png", "#ff695b")
        {
            EditCommand = new RelayCommand(
                _ => onEdit());

            RemoveCommand = new RelayCommand(
                _ => onRemove());
        }
    }
}
