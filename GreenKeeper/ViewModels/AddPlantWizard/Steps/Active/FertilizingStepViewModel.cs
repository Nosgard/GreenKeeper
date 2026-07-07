using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.ViewModels.AddPlantWizard.Steps.Active
{
    public class FertilizingStepViewModel : ActiveStepViewModel
    {
        // Optional field: The button is alway active, no matter if
        // there is a value or not. The difference is only the label
        public override bool CanProceed => true;

        // As long as there is no valid number: "Skip"
        // Valid number entered: "Next"
        public override string NextButtonLabel => HasValidAmount ? "Next" : "Skip";
    }
}
