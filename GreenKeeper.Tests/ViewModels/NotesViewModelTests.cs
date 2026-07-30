using GreenKeeper.Models;
using GreenKeeper.Tests.Fakes;
using GreenKeeper.ViewModels.Notes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.Tests.ViewModels
{
    public class NotesViewModelTests
    {
        [Fact]
        public void Constructor_GivenPlantWithExistingNotes_InitializesEditableNotesAndIsNotDirty()
        {
            // Given: a plant with an existing note text
            var plant = new Plant { Name = "Aloe Vera", Notes = "Loves indirect sunlight." };
            var dialogService = new FakeDialogService();

            // A callback stand-in for the actual save logic - not expected to
            // be called in this test, since we're only checking the initial state
            Func<string, Task> saveNotesAsync = _ => Task.CompletedTask;

            // When: a NotesViewModel is created for this plant
            var viewModel = new NotesViewModel(plant, dialogService, saveNotesAsync);

            // Then: EditableNotes should reflect the plant's existing notes,
            // and nothing has been changed yet
            Assert.Equal("Loves indirect sunlight.", viewModel.EditableNotes);
            Assert.False(viewModel.IsDirty);
        }

        [Fact]
        public void SaveCommand_GivenModifiedNotes_PersistsViaCallbackAndClearsIsDirty()
        {
            // Given a plant with existing notes, and EditableNotes changed to a new value
            var plant = new Plant { Name = "Aloe Vera", Notes ="Old notes"};
            var dialogService = new FakeDialogService();

            // Captures whatever text the ViewModel attempts to save, so the test
            // can verify it without needing a real repository/database
            string? savedText = null;
            Func<string, Task> saveNotesAsync = text =>
            {
                savedText = text;
                return Task.CompletedTask;
            };

            var viewModel = new NotesViewModel(plant, dialogService, saveNotesAsync);
            viewModel.EditableNotes = "New notes";

            // When: SaveCommand is executed
            viewModel.SaveCommand.Execute(null);

            // Then: the callback received the new text, and IsDirty is now false
            // (the newly saved text counts as the current "original" state)
            Assert.Equal("New notes", savedText);
            Assert.False(viewModel.IsDirty);
        }

        [Fact]
        public void SaveCommand_GivenUnmodifiedNotes_CanExecuteReturnsFalse()
        {
            // Given: a plant with existing notes, EditableNotes left untouched (IsDirty == false)
            var plant = new Plant { Name = "Aloe Vera", Notes = "Unchanged notes" };
            var dialogService = new FakeDialogService();
            Func<string, Task> saveNotesAsync = _ => Task.CompletedTask;

            // When: a NotesViewModel is created for this plant
            var viewModel = new NotesViewModel(plant, dialogService, saveNotesAsync);

            // Then: SaveCommand should not be executable, since nothing changed
            Assert.False(viewModel.SaveCommand.CanExecute(null));
        }

        [Fact]
        public void CancelCommand_GivenUnmodifiedNotes_ClosesImmediatelyWithoutConfirmation()
        {
            // Given: a plant with notes, EditableNotes left untouched (IsDirty == false)
            var plant = new Plant { Name = "Aloe Vera", Notes = "Unchanged notes" };
            var dialogService = new FakeDialogService();
            Func<string, Task> saveNotesAsync = _ => Task.CompletedTask;

            var viewModel = new NotesViewModel(plant, dialogService, saveNotesAsync);

            bool? closeResult = null;
            viewModel.RequestClose += (_, result) => closeResult = result;

            // When: CancelCommand is executed
            viewModel.CancelCommand.Execute(null);

            // Then: the window closes immediately with false, no confirmation dialog needed
            Assert.False(dialogService.ConfirmWasCalled);
            Assert.Equal(false, closeResult);
        }

        [Fact]
        public void CancelCommand_GivenModifiedNotesAndUserConfirmsSave_SavesAndClosesWithTrue()
        {
            // Given: a plant with notes, EditableNotes changed to a new value,
            // and the dialog service configured to simulate the user choosing "Yes"
            var plant = new Plant { Name = "Aloe Vera", Notes = "Old notes" };
            var dialogService = new FakeDialogService { ConfirmResult = true };

            string? savedText = null;
            Func<string, Task> saveNotesAsync = text =>
            {
                savedText = text;
                return Task.CompletedTask;
            };

            var viewModel = new NotesViewModel(plant, dialogService, saveNotesAsync);
            viewModel.EditableNotes = "New notes";

            bool? closeResult = null;
            viewModel.RequestClose += (_, result) => closeResult = result;

            // When: CancelCommand is executed
            viewModel.CancelCommand.Execute(null);

            // Then: the user was asked, the new text was saved, and the window
            // closes with true (saved)
            Assert.True(dialogService.ConfirmWasCalled);
            Assert.Equal("New notes", savedText);
            Assert.Equal(true, closeResult);
        }

        [Fact]
        public void CancelCommand_GivenModifiedNotesAndUserDeclinesSave_DiscardsAndClosesWithFalse()
        {
            // Given: a plant with notes, EditableNotes changed to a new value,
            // and the dialog service configured to simulate the user choosing "No"
            var plant = new Plant { Name = "Aloe Vera", Notes = "Old notes" };
            var dialogService = new FakeDialogService { ConfirmResult = false };

            bool saveWasCalled = false;
            Func<string, Task> saveNotesAsync = _ =>
            {
                saveWasCalled = true;
                return Task.CompletedTask;
            };

            var viewModel = new NotesViewModel(plant, dialogService, saveNotesAsync);
            viewModel.EditableNotes = "New notes";

            bool? closeResult = null;
            viewModel.RequestClose += (_, result) => closeResult = result;

            // When: CancelCommand is executed
            viewModel.CancelCommand.Execute(null);

            // Then: the user was asked, but the change was discarded (never saved),
            // and the window closes with false
            Assert.True(dialogService.ConfirmWasCalled);
            Assert.False(saveWasCalled);
            Assert.Equal(false, closeResult);
        }

        [Fact]
        public void SaveCommand_GivenCallbackThrows_ShowsErrorAndKeepsChangesUnsaved()
        {
            // Given: a plant with notes, EditableNotes changed, and a
            // callback that simulates a failing save (e.g. a database error)
            var plant = new Plant { Name = "Aloe Vera", Notes = "Old notes" };
            var dialogService = new FakeDialogService();

            Func<string, Task> saveNotesAsync = _ => throw new InvalidOperationException("Simulated save failure");

            var viewModel = new NotesViewModel(plant, dialogService, saveNotesAsync);
            viewModel.EditableNotes = "New notes";

            bool closeWasRequested = false;
            viewModel.RequestClose += (_, _) => closeWasRequested = true;

            // When: SaveCommand is executed
            viewModel.SaveCommand.Execute(null);

            // Then: an error is shown, the change is NOT marked as saved, and the
            // dialog stays open (no RequestClose) so the user doesn't lose their input
            Assert.True(dialogService.ShowErrorWasCalled);
            Assert.True(viewModel.IsDirty);
            Assert.False(closeWasRequested);
        }
    }
}
