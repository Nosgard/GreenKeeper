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
        /// Set the status card for Fertilizing via the given schedule.
        /// onEdit: Is provided by MainViewModel and encapsulates the EditScheduleView for this care type
        /// onRemove: Is provided by MainViewModel and encapsulates the confirmation + removal.
        /// 
        /// Note: The ViewModel knows neither the plant object nor any window class;
        /// it only triggers the given Action.
        /// </summary>
        public FertilizingStatusViewModel(CareSchedule? schedule, Action onComplete, Action onEdit, Action onRemove)
            : base(CareType.Fertilizing, schedule, "Fertilizing", "/Resources/Icons/Pill.png", "#ff695b")
        {
            CompleteCommand = new RelayCommand(
                _ => onComplete());

            EditCommand = new RelayCommand(
                _ => onEdit());

            RemoveCommand = new RelayCommand(
                _ => onRemove());
        }
    }
}
