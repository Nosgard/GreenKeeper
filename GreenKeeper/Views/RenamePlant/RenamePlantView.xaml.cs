using GreenKeeper.Models;
using GreenKeeper.ViewModels.RenamePlant;

namespace GreenKeeper.Views.RenamePlant
{
    /// <summary>
    /// Interaction logic for RenamePlantView.xaml.
    /// </summary>
    public partial class RenamePlantView : DialogWindow
    {
        private readonly RenamePlantViewModel _renamePlantViewModel;

        public RenamePlantView(Plant plant)
        {
            InitializeComponent();
            _renamePlantViewModel = new RenamePlantViewModel(plant);
            Attach(_renamePlantViewModel);

            // Puts the cursor straight into the input field,
            // so the user can continue typing at the end of the current name.
            Loaded += (_, _) =>
            {
                NameTextBox.Focus();
                NameTextBox.CaretIndex = NameTextBox.Text.Length;
            };
        }

        // Exposes the name to the caller (MainWindow), analogous to
        // EditScheduleView.Result - the actual persistence happens in the MainViewModel.
        public string? ConfirmedName => _renamePlantViewModel.ConfirmedName;
    }
}
