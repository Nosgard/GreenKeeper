using GreenKeeper.Models;
using GreenKeeper.ViewModels.RenamePlant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

            // Then: the counter reflects the remaining budget of the 50 character limit
            Assert.Equal(RenamePlantViewModel.MaxNameLength - 9, viewModel.CharactersRemaining);
        }

        [Fact]
        public void Constructor_GivenPlant_LeavesConfirmedNameNull()
        {
            // Given: a plant
            var plant = new Plant { Name = "Aloe Vera" };

            // When: a RenamePlantViewModel is created, but nothing confirmed yet
            var viewModel = new RenamePlantViewModel(plant);

            // Then: ConfirmedName stays null until the user actually saves
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

            // Then: all three properties that depend on the name must notify,
            // otherwise the input field, the counter and the Save button's
            // enabled state would fall out of sync in the UI
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

            // Then: the counter reflects the new length, not the original one
            Assert.Equal(RenamePlantViewModel.MaxNameLength - 5, viewModel.CharactersRemaining);
        }

        // -- Save Command Validation Tests --

        [Fact]
        public void SaveCommand_GivenValidName_CanExecuteReturnsTrue()
        {
            // Given: a ViewModel with a non-empty name
            var plant = new Plant { Name = "Aloe Vera" };
            var viewModel = new RenamePlantViewModel(plant);

            // When / Then: saving should be possible
            Assert.True(viewModel.SaveCommand.CanExecute(null));
        }

        [Fact]
        public void SaveCommand_GivenEmptyName_CanExecuteReturnsFalse()
        {
            // Given: a ViewModel whose name was cleared entirely
            var plant = new Plant { Name = "Aloe Vera" };
            var viewModel = new RenamePlantViewModel(plant);

            // When: saving must be blocked
            viewModel.NewName = string.Empty;

            // Then: a plant without a name would show up
            // as an empty entry in the sidebar
            Assert.False(viewModel.SaveCommand.CanExecute(null));
        }

        /// <summary>
        /// Covers the specific reason HasValidName uses IsNullOrWhiteSpace
        /// rather than IsNullOrEmpty: a name made purely of spaces passes an
        /// empty-check but would still look blank in the sidebar.
        /// </summary>
        [Fact]
        public void SaveCommand_GivenWhitespaceOnlyName_CanExecuteReturnsFalse()
        {
            // Given: a ViewModel whose name consists only of spaces
            var plant = new Plant { Name = "Aloe Vera" };
            var viewModel = new RenamePlantViewModel(plant);

            // When: saving must be blocked
            viewModel.NewName = "   ";

            // Then: show as an empty entry
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

            // Then: the stored name is trimmed - invisible leading/trailing
            // spaces would otherwise end up in the database and affect
            // sorting and searching
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
