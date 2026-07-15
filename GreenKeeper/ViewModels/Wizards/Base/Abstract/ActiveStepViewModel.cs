using GreenKeeper.Models.Enums;
using GreenKeeper.ViewModels.Base;
using GreenKeeper.ViewModels.Wizards.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.ViewModels.Wizards.Base.Abstract
{
    public abstract class ActiveStepViewModel : AmountAndUnitInputViewModel, IWizardStepViewModel
    {
        // Mandatory for watering + optional for fertilizing.
        // Will be implemented by the affected class
        public abstract bool CanProceed { get; }

        // Standard: "Next"
        // In the context of fertilizing: "Skip" or "Next"
        public virtual string NextButtonLabel => "Next";


        protected override void OnAmountOrUnitChanged()
        {
            OnPropertyChanged(nameof(CanProceed));
            OnPropertyChanged(nameof(NextButtonLabel));
        }
    }
}
