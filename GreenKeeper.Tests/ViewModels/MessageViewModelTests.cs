using GreenKeeper.ViewModels.Messages;

namespace GreenKeeper.Tests.ViewModels
{
    /// <summary>
    /// The answer of the message window, which every question asked through
    /// MessageDialogService.Confirm depends on.
    /// </summary>
    public class MessageViewModelTests
    {
        [Fact]
        public void ConfirmCommand_WhenExecuted_ClosesWithTrue()
        {
            // Given: a question with a listener for its answer
            var viewModel = new MessageViewModel("Delete the plant?", isQuestion: true);
            bool? answer = null;
            viewModel.RequestClose += (_, result) => answer = result;

            // When: the user confirms
            viewModel.ConfirmCommand.Execute(null);

            // Then: the window closes with a yes
            Assert.Equal(true, answer);
        }

        [Fact]
        public void DeclineCommand_WhenExecuted_ClosesWithFalse()
        {
            // Given: a question with a listener for its answer
            var viewModel = new MessageViewModel("Delete the plant?", isQuestion: true);
            bool? answer = null;
            viewModel.RequestClose += (_, result) => answer = result;

            // When: the user declines
            viewModel.DeclineCommand.Execute(null);

            // Then: the window closes with a no
            Assert.Equal(false, answer);
        }
    }
}
