using GreenKeeper.Models;
using GreenKeeper.ViewModels.Wizards.AddPlantWizard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GreenKeeper.Views.Wizards.AddPlantWizard
{
    /// <summary>
    /// Interaction logic for AddPlantWizardView.xaml
    /// </summary>
    public partial class AddPlantWizardView : Window
    {
        private readonly AddPlantWizardViewModel _addPlantWizardViewModel;

        public AddPlantWizardView()
        {
            InitializeComponent();
            _addPlantWizardViewModel = new AddPlantWizardViewModel();
            _addPlantWizardViewModel.RequestClose += AddPlantWizardViewModel_RequestClose;
            this.DataContext = _addPlantWizardViewModel;
        }

        // After the Wizard is finished, read the created Plant-Object from the ViewModel.
        // The Plant-Object is null if the Wizard was canceled
        public Plant? CreatedPlant => _addPlantWizardViewModel.CreatedPlant;

        private void AddPlantWizardViewModel_RequestClose(object? sender, bool dialogResult)
        {
            _addPlantWizardViewModel.RequestClose -= AddPlantWizardViewModel_RequestClose;
            DialogResult = dialogResult;
        }
    }
}
