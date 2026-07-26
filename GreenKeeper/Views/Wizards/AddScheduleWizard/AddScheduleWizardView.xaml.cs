using GreenKeeper.Models;
using GreenKeeper.Services;
using GreenKeeper.ViewModels.Wizards.AddScheduleWizard;
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

namespace GreenKeeper.Views.Wizards.AddScheduleWizard
{
    /// <summary>
    /// Interaction logic for AddScheduleWizardView.xaml
    /// </summary>
    public partial class AddScheduleWizardView : Window
    {
        private readonly AddScheduleWizardViewModel _addScheduleWizardViewModel;
        public AddScheduleWizardView(Plant plant, IDialogService dialogService)
        {
            InitializeComponent();
            _addScheduleWizardViewModel = new AddScheduleWizardViewModel(plant, dialogService);
            _addScheduleWizardViewModel.RequestClose += ViewModel_RequestClose;
            this.DataContext = _addScheduleWizardViewModel;
        }

        // Exposes the Wizard's result to the caller (MainWindow), analogous to
        // AddPlantWizard.CreatedPlant - persistence itself happens outside this View, in MainViewModel
        public CareSchedule? CreatedCareSchedule => _addScheduleWizardViewModel.CreatedCareSchedule;
        public SunlightRequirement? CreatedSunlightRequirement => _addScheduleWizardViewModel.CreatedSunlightRequirement;
        private void ViewModel_RequestClose(object? sender, bool dialogResult)
        {
            _addScheduleWizardViewModel.RequestClose -= ViewModel_RequestClose;
            DialogResult = dialogResult;
        }
    }
}
