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
    /// Interaction logic for AddPlantWizardView.xaml.
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

        // After the wizard is finished, read the created plant object from the ViewModel.
        // The plant object is null if the wizard was canceled
        public Plant? CreatedPlant => _addPlantWizardViewModel.CreatedPlant;

        private void AddPlantWizardViewModel_RequestClose(object? sender, bool dialogResult)
        {
            _addPlantWizardViewModel.RequestClose -= AddPlantWizardViewModel_RequestClose;
            DialogResult = dialogResult;
        }
    }
}
