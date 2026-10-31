using GreenKeeper.Models;
using GreenKeeper.ViewModels.RenamePlant;

namespace GreenKeeper.Views.RenamePlant
{
    public partial class RenamePlantView : DialogWindow
    {
        private readonly RenamePlantViewModel _renamePlantViewModel;

        public RenamePlantView(Plant plant)
        {
            InitializeComponent();
            _renamePlantViewModel = new RenamePlantViewModel(plant);
            Attach(_renamePlantViewModel);
        }

        // The confirmed name for MainWindow, which saves it through PlantListViewModel.
        public string? ConfirmedName => _renamePlantViewModel.ConfirmedName;
    }
}
