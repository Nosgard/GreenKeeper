using GreenKeeper.Models;
using GreenKeeper.ViewModels.Wizards.AddPlantWizard;

namespace GreenKeeper.Views.Wizards.AddPlantWizard
{
    /// <summary>
    /// Interaction logic for AddPlantWizardView.xaml.
    /// </summary>
    public partial class AddPlantWizardView : DialogWindow
    {
        private readonly AddPlantWizardViewModel _addPlantWizardViewModel;

        public AddPlantWizardView()
        {
            InitializeComponent();
            _addPlantWizardViewModel = new AddPlantWizardViewModel();
            Attach(_addPlantWizardViewModel);
        }

        // After the wizard is finished, read the created plant object from the ViewModel.
        // The plant object is null if the wizard was canceled.
        public Plant? CreatedPlant => _addPlantWizardViewModel.CreatedPlant;
    }
}
