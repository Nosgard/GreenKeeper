using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.ViewModels.AddPlantWizard.Steps
{
    public interface IWizardStepViewModel : INotifyPropertyChanged
    {
        // Controls the "Next"-Button on every page whether to be active or not
        bool CanProceed { get; }
    }
}
