using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.Scheduling;
using GreenKeeper.ViewModels.CareStatuses.EditOption;

namespace GreenKeeper.Views.CareStatuses.EditOption
{
    /// <summary>
    /// Interaction logic for EditScheduleView.xaml.
    /// </summary>
    public partial class EditScheduleView : DialogWindow
    {
        private readonly EditScheduleViewModel _viewModel;

        // The parameters determine which care schedule or sunlight requirement gets edited.
        public EditScheduleView(Plant plant, CareType careType, TimeProvider timeProvider)
        {
            InitializeComponent();
            _viewModel = new EditScheduleViewModel(plant, careType, timeProvider);
            Attach(_viewModel);
        }

        // Exposes the edited entry to the caller (MainWindow); the persistence happens in the MainViewModel.
        public ScheduleInput? Result => _viewModel.Result;
    }
}
