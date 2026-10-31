using GreenKeeper.Models;
using GreenKeeper.ViewModels.RenamePlant;

namespace GreenKeeper.Tests.ViewModels
{
    public class RenamePlantViewModelTests
    {
        // -- Initialization Tests --

        [Fact]
        public void Constructor_GivenPlantWithName_PRefillsNewNameWithCurrentName()
        {
            // Given: a plant with an existing name
            var plant = new Plant { Name = "Aloe Vera" };

            // When: a RenamePlantViewModel is created for it
            var viewModel = new RenamePlantViewModel(plant);

            // Then: the input field starts out with the plant's current name,
            // so the user sees what's about to be changed
            Assert.Equal("Aloe Vera", viewModel.NewName);
        }

        [Fact]
        public void Constructor_GivenPlantWithName_CalculatesInitialCharactersRemaining()
        {
            // Given: a plant whose name is 9 characters long
            var plant = new Plant { Name = "Aloe Vera" };

            // When: a RenamePlantViewModel is created for it
            var viewModel = new RenamePlantViewModel(plant);

            // Then: 41 of the 50 characters are left
            Assert.Equal(Plant.MaxNameLength - "Aloe Vera".Length, viewModel.CharactersRemaining);
        }

        [Fact]
        public void Constructor_GivenPlant_LeavesConfirmedNameNull()
        {
            // Given: a plant
            var plant = new Plant { Name = "Aloe Vera" };

            // When: a RenamePlantViewModel is created, but nothing is confirmed yet
            var viewModel = new RenamePlantViewModel(plant);

            // Then: ConfirmedName stays null until the user saves
            Assert.Null(viewModel.ConfirmedName);
        }

        // -- Property Changed Tests --

        [Fact]
        public void NewName_WhenChanged_RaisesPropertyChangedForAllDependentProperties()
        {
            // Given: a ViewModel with a PropertyChanged listener attached
            var plant = new Plant { Name = "Aloe Vera" };
            var viewModel = new RenamePlantViewModel(plant);

            var raisedProperties = new List<string>();
            viewModel.PropertyChanged += (_, e) => raisedProperties.Add(e.PropertyName!);

            // When: the name is changed
            viewModel.NewName = "Basil";

            // Then: PropertyChanged was raised for NewName, CharactersRemaining and HasValidName
            Assert.Contains(nameof(RenamePlantViewModel.NewName), raisedProperties);
            Assert.Contains(nameof(RenamePlantViewModel.CharactersRemaining), raisedProperties);
            Assert.Contains(nameof(RenamePlantViewModel.HasValidName), raisedProperties);
        }

        [Fact]
        public void CharactersRemaining_WhenNameChanged_UpdatesAccordingly()
        {
            // Given: a ViewModel initialized with a plant
            var plant = new Plant { Name = "Aloe Vera" };
            var viewModel = new RenamePlantViewModel(plant);

            // When: a shorter name is entered
            viewModel.NewName = "Basil";

            // Then: the counter follows the new name: 45 of the 50 characters are left
            Assert.Equal(Plant.MaxNameLength - "Basil".Length, viewModel.CharactersRemaining);
        }

        // -- Save Command Validation Tests --

        [Fact]
        public void SaveCommand_GivenValidName_CanExecuteReturnsTrue()
        {
            // Given: a ViewModel with a non-empty name
            var plant = new Plant { Name = "Aloe Vera" };
            var viewModel = new RenamePlantViewModel(plant);

            // When: the enabled state of the Save button is read
            var canSave = viewModel.SaveCommand.CanExecute(null);

            // Then: saving is allowed
            Assert.True(canSave);
        }

        [Fact]
        public void SaveCommand_GivenEmptyName_CanExecuteReturnsFalse()
        {
            // Given: a ViewModel for a plant with a name
            var plant = new Plant { Name = "Aloe Vera" };
            var viewModel = new RenamePlantViewModel(plant);

            // When: the name is cleared
            viewModel.NewName = string.Empty;

            // Then: saving is blocked, since a plant without a name would show up as an empty
            // entry in the sidebar
            Assert.False(viewModel.SaveCommand.CanExecute(null));
        }

        /// <summary>
        /// A name of spaces only passes an empty check but looks blank in the sidebar, so it has
        /// to be refused like an empty one (see Plant.IsValidName).
        /// </summary>
        [Fact]
        public void SaveCommand_GivenWhitespaceOnlyName_CanExecuteReturnsFalse()
        {
            // Given: a ViewModel for a plant with a name
            var plant = new Plant { Name = "Aloe Vera" };
            var viewModel = new RenamePlantViewModel(plant);

            // When: the name is replaced by spaces
            viewModel.NewName = "   ";

            // Then: saving is blocked, since the name would show as an empty entry
            Assert.False(viewModel.SaveCommand.CanExecute(null));
        }

        // -- Save Command Execution Tests --

        [Fact]
        public void SaveCommand_GivenNewName_SetsConfirmedNameAndClosesWithTrue()
        {
            // Given: a ViewModel with a new name entered
            var plant = new Plant { Name = "Aloe Vera" };
            var viewModel = new RenamePlantViewModel(plant);
            viewModel.NewName = "Basil";

            bool? closeResult = null;
            viewModel.RequestClose += (_, result) => closeResult = result;

            // When: SaveCommand is executed
            viewModel.SaveCommand.Execute(null);

            // Then: the new name is handed over to the caller, and the dialog
            // closes with true (confirmed)
            Assert.Equal("Basil", viewModel.ConfirmedName);
            Assert.Equal(true, closeResult);
        }

        [Fact]
        public void SaveCommand_GivenNameWithSurroundingWhitespace_TrimsConfirmedName()
        {
            // Given: a ViewModel with a name entered that has stray spaces
            var plant = new Plant { Name = "Aloe Vera" };
            var viewModel = new RenamePlantViewModel(plant);
            viewModel.NewName = "  Basil  ";

            // When: SaveCommand is executed
            viewModel.SaveCommand.Execute(null);

            // Then: the confirmed name comes without the spaces at its start and end
            Assert.Equal("Basil", viewModel.ConfirmedName);
        }

        // -- Cancel Command Tests --

        [Fact]
        public void CancelCommand_WhenExecuted_LeavesConfirmedNameNullAndClosesWithFalse()
        {
            // Given: a ViewModel with a new name entered but not saved
            var plant = new Plant { Name = "Aloe Vera" };
            var viewModel = new RenamePlantViewModel(plant);
            viewModel.NewName = "Basil";

            bool? closeResult = null;
            viewModel.RequestClose += (_, result) => closeResult = result;

            // When: CancelCommand is executed
            viewModel.CancelCommand.Execute(null);

            // Then: nothing is handed over to the caller, and the dialog
            // closes with false (discarded)
            Assert.Null(viewModel.ConfirmedName);
            Assert.Equal(false, closeResult);
        }

    }
}
