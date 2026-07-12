using GreenKeeper.ViewModels.AddPlantWizard.Steps.Active;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.ViewModels.AddScheduleWizard
{
    /// <summary>
    /// Will be used for both Watering and Fertilizing, if the user
    /// decided for one of both statuses.
    /// Unlike in the AddPlantWizard, entering a value is always mandatory
    /// because the user selected the status on purpose
    /// </summary>
    public class ScheduleActiveStepViewModel : ActiveStepViewModel
    {
        public string Title { get; }

        public ScheduleActiveStepViewModel(string title)
        {
            Title = title;
        }

        public override bool CanProceed => HasValidAmount;

        public override string NextButtonLabel => "Finish";
    }
}
