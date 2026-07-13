using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.ViewModels.Wizards.Base
{
    public interface IWizardStepViewModel : INotifyPropertyChanged
    {
        // Controls the "Next"-Button on every page whether to be active or not
        bool CanProceed { get; }

        // Controls the text of the "Next"-Button.
        // Usually "Next", in case of optional and empty steps "Skip"
        string NextButtonLabel { get; }
    }
}
