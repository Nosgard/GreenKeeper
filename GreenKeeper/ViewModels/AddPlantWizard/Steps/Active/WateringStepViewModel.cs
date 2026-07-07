using GreenKeeper.Models.Enums;
using GreenKeeper.ViewModels.AddPlantWizard.Steps.Active;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.ViewModels.AddPlantWizard.Steps.Active
{
    class WateringStepViewModel : ActiveStepViewModel
    {
        // Mandatory field: Next will be active, when a valid number was entered
        public override bool CanProceed => HasValidAmount;
    }
}
