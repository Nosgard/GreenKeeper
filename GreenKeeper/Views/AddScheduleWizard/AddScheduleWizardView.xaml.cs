using GreenKeeper.Models;
using GreenKeeper.Services;
using GreenKeeper.ViewModels.AddScheduleWizard;
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

namespace GreenKeeper.Views.AddScheduleWizard
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

        private void ViewModel_RequestClose(object? sender, bool dialogResult)
        {
            _addScheduleWizardViewModel.RequestClose -= ViewModel_RequestClose;
            DialogResult = dialogResult;
        }
    }
}
