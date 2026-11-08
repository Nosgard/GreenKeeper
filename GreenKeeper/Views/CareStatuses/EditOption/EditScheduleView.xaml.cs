using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.Scheduling;
using GreenKeeper.ViewModels.CareStatuses.EditOption;

namespace GreenKeeper.Views.CareStatuses.EditOption
{
    public partial class EditScheduleView : DialogWindow
    {
        private readonly EditScheduleViewModel _viewModel;

        public EditScheduleView(Plant plant, CareType careType, TimeProvider timeProvider)
        {
            InitializeComponent();
            _viewModel = new EditScheduleViewModel(plant, careType, timeProvider);
            Attach(_viewModel);
        }

        // The edited entry for MainWindow, which saves it through DashboardViewModel.
        public ScheduleInput? Result => _viewModel.Result;
    }
}
