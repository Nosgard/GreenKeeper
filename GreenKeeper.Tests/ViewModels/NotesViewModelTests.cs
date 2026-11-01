using GreenKeeper.Models;
using GreenKeeper.Services;
using GreenKeeper.Tests.Fakes;
using GreenKeeper.ViewModels.Notes;

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

            // A save callback this test never calls, since it only checks the start state.
            Func<string, Task> saveNotesAsync = _ => Task.CompletedTask;

            // When: a NotesViewModel is created for this plant
            var viewModel = new NotesViewModel(plant, dialogService, saveNotesAsync);

            // Then: EditableNotes holds the plant's existing notes,
            // and nothing has been changed yet
            Assert.Equal("Loves indirect sunlight.", viewModel.EditableNotes);
            Assert.False(viewModel.IsDirty);
        }

        [Fact]
        public void Constructor_GivenPlantWithoutNotes_StartsWithEmptyTextAndIsNotDirty()
        {
            // Given: a plant that has no notes yet
            var plant = new Plant { Name = "Aloe Vera", Notes = null };
            var dialogService = new FakeDialogService();
            Func<string, Task> saveNotesAsync = _ => Task.CompletedTask;

            // When: a NotesViewModel is created for this plant
            var viewModel = new NotesViewModel(plant, dialogService, saveNotesAsync);

            // Then: the text starts empty instead of missing, and nothing counts as changed
            Assert.Equal(string.Empty, viewModel.EditableNotes);
            Assert.False(viewModel.IsDirty);
        }

        [Fact]
        public async Task SaveCommand_GivenModifiedNotes_PersistsViaCallbackAndClearsIsDirty()
        {
            // Given: a plant with existing notes, and EditableNotes changed to a new value
            var plant = new Plant { Name = "Aloe Vera", Notes = "Old notes" };
            var dialogService = new FakeDialogService();

            // Records the text the ViewModel saves.
            string? savedText = null;
            Func<string, Task> saveNotesAsync = text =>
            {
                savedText = text;
                return Task.CompletedTask;
            };

            var viewModel = new NotesViewModel(plant, dialogService, saveNotesAsync);
            viewModel.EditableNotes = "New notes";

            // When: SaveCommand is executed
            await viewModel.SaveCommand.ExecuteAsync(null);

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

            // Then: SaveCommand cannot run, since nothing changed
            Assert.False(viewModel.SaveCommand.CanExecute(null));
        }

        [Fact]
        public async Task CancelCommand_GivenUnmodifiedNotes_ClosesImmediatelyWithoutConfirmation()
        {
            // Given: a plant with notes, EditableNotes left untouched (IsDirty == false)
            var plant = new Plant { Name = "Aloe Vera", Notes = "Unchanged notes" };
            var dialogService = new FakeDialogService();
            Func<string, Task> saveNotesAsync = _ => Task.CompletedTask;

            var viewModel = new NotesViewModel(plant, dialogService, saveNotesAsync);

            bool? closeResult = null;
            viewModel.RequestClose += (_, result) => closeResult = result;

            // When: CancelCommand is executed
            await viewModel.CancelCommand.ExecuteAsync(null);

            // Then: the window closes immediately with false, no confirmation dialog needed
            Assert.False(dialogService.ConfirmWasCalled);
            Assert.Equal(false, closeResult);
        }

        [Fact]
        public async Task CancelCommand_GivenModifiedNotesAndUserConfirmsSave_SavesAndClosesWithTrue()
        {
            // Given: a plant with notes, EditableNotes changed to a new value,
            // and the user answering "Yes" when asked to save
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
            await viewModel.CancelCommand.ExecuteAsync(null);

            // Then: the user was asked, the new text was saved, and the window
            // closes with true (saved)
            Assert.True(dialogService.ConfirmWasCalled);
            Assert.Equal("New notes", savedText);
            Assert.Equal(true, closeResult);
        }

        [Fact]
        public async Task CancelCommand_GivenModifiedNotes_OffersYesFirst()
        {
            // Given: notes that were changed
            var plant = new Plant { Name = "Aloe Vera", Notes = "Old notes" };
            var dialogService = new FakeDialogService { ConfirmResult = true };
            var viewModel = new NotesViewModel(plant, dialogService, _ => Task.CompletedTask);
            viewModel.EditableNotes = "New notes";

            // When: the window is closed and asks "Do you want to save?"
            await viewModel.CancelCommand.ExecuteAsync(null);

            // Then: "No" would discard the changes here, so "Yes" is offered first
            Assert.Equal(Answer.Yes, dialogService.OfferedAnswer);
        }

        [Fact]
        public async Task CancelCommand_GivenModifiedNotesAndUserDeclinesSave_DiscardsAndClosesWithFalse()
        {
            // Given: a plant with notes, EditableNotes changed to a new value,
            // and the user answering "No" when asked to save
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

            var closeResults = new List<bool?>();
            viewModel.RequestClose += (_, result) => closeResults.Add(result);

            // When: CancelCommand is executed
            await viewModel.CancelCommand.ExecuteAsync(null);

            // Then: the user was asked, but the change was discarded (never saved),
            // and the window closes exactly once with false
            Assert.True(dialogService.ConfirmWasCalled);
            Assert.False(saveWasCalled);
            Assert.Equal(new bool?[] { false }, closeResults);
        }

        [Theory]
        [InlineData(false, 1)]   // the save works -> one signal
        [InlineData(true, 0)]    // the save fails -> no signal
        public async Task SaveCommand_GivenSaveWorksOrFails_SignalsSavedOnlyWhenItWorked(bool callbackThrows, int expectedSignals)
        {
            // Given: changed notes, a save that works or fails, and a listener for the signal
            var plant = new Plant { Name = "Aloe Vera", Notes = "Old notes" };
            Func<string, Task> saveNotesAsync = _ =>
                callbackThrows ? throw new InvalidOperationException("Simulated save failure") : Task.CompletedTask;

            var viewModel = new NotesViewModel(plant, new FakeDialogService(), saveNotesAsync);
            viewModel.EditableNotes = "New notes";

            int signals = 0;
            viewModel.Saved += (_, _) => signals++;

            // When: SaveCommand is executed
            await viewModel.SaveCommand.ExecuteAsync(null);

            // Then: Saved is raised once after a successful save, never after a failure.
            // The window stays open, so the disabled Save button would be the only other sign
            Assert.Equal(expectedSignals, signals);
        }

        [Fact]
        public async Task SaveCommand_GivenCallbackThrows_ShowsErrorAndKeepsChangesUnsaved()
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
            await viewModel.SaveCommand.ExecuteAsync(null);

            // Then: an error is shown, the change still counts as unsaved, and the dialog stays
            // open (no RequestClose), so the user keeps their input
            Assert.True(dialogService.ShowErrorWasCalled);
            Assert.True(viewModel.IsDirty);
            Assert.False(closeWasRequested);
        }

        [Fact]
        public async Task CancelCommand_GivenModifiedNotesAndSavingFails_ShowsErrorAndKeepsTheWindowOpen()
        {
            // Given: a plant with notes, EditableNotes changed, the user answering "Yes"
            // when asked to save, and a callback that simulates a failing save
            var plant = new Plant { Name = "Aloe Vera", Notes = "Old notes" };
            var dialogService = new FakeDialogService { ConfirmResult = true };

            Func<string, Task> saveNotesAsync = _ => throw new InvalidOperationException("Simulated save failure");

            var viewModel = new NotesViewModel(plant, dialogService, saveNotesAsync);
            viewModel.EditableNotes = "New notes";

            bool closeWasRequested = false;
            viewModel.RequestClose += (_, _) => closeWasRequested = true;

            // When: CancelCommand is executed
            await viewModel.CancelCommand.ExecuteAsync(null);

            // Then: an error is shown, and the window stays open with the change still unsaved;
            // closing now would throw away the text the user asked to keep
            Assert.True(dialogService.ShowErrorWasCalled);
            Assert.True(viewModel.IsDirty);
            Assert.False(closeWasRequested);
        }
    }
}
