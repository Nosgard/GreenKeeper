using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.ViewModels.CareStatuses.EditOption;
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

namespace GreenKeeper.Views.CareStatuses.EditOption
{
    /// <summary>
    /// Interaction logic for EditScheduleView.xaml
    /// </summary>
    public partial class EditScheduleView : Window
    {
        private readonly EditScheduleViewModel _viewModel;

        // The parameters determine which Care-Schedule or Sunlight-Requirement gets edited
        public EditScheduleView(Plant plant, CareType careType)
        {
            InitializeComponent();
            _viewModel = new EditScheduleViewModel(plant, careType);
            _viewModel.RequestClose += ViewModel_RequestClose;
            this.DataContext = _viewModel;
        }

        private void ViewModel_RequestClose(object? sender, bool dialogResult)
        {
            _viewModel.RequestClose -= ViewModel_RequestClose;
            DialogResult = dialogResult;
        }
    }
}
