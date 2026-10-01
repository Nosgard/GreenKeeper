using GreenKeeper.Commands;

namespace GreenKeeper.Tests.Commands
{
    public class AsyncRelayCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_RunsTheDelegateWithTheParameter()
        {
            // Given: a command whose delegate records what it was called with
            object? received = null;
            var command = new AsyncRelayCommand(parameter =>
            {
                received = parameter;
                return Task.CompletedTask;
            });

            // When: it is executed with a parameter
            await command.ExecuteAsync("plant");

            // Then: the delegate ran with exactly that parameter
            Assert.Equal("plant", received);
        }

        [Fact]
        public async Task ExecuteAsync_AwaitsTheDelegate()
        {
            // Given: a delegate that only completes when the test lets it
            var completion = new TaskCompletionSource();
            var command = new AsyncRelayCommand(_ => completion.Task);

            // When: the command is executed
            var execution = command.ExecuteAsync(null);

            // Then: the execution is not over before the delegate is
            Assert.False(execution.IsCompleted);
            completion.SetResult();
            await execution;
            Assert.True(execution.IsCompleted);
        }

        [Fact]
        public void Execute_RunsTheDelegate()
        {
            // Given: a command bound the way WPF binds it, through ICommand
            bool wasRun = false;
            System.Windows.Input.ICommand command = new AsyncRelayCommand(_ =>
            {
                wasRun = true;
                return Task.CompletedTask;
            });

            // When: the ICommand entry point is used
            command.Execute(null);

            // Then: the delegate ran
            Assert.True(wasRun);
        }

        [Fact]
        public async Task ExecuteAsync_GivenDelegateThrows_PropagatesTheException()
        {
            // Given: a delegate that fails
            var command = new AsyncRelayCommand(_ => throw new InvalidOperationException("Simulated failure"));

            // When: the command is executed
            var exception = await Record.ExceptionAsync(() => command.ExecuteAsync(null));

            // Then: the caller sees the failure instead of it getting lost
            Assert.IsType<InvalidOperationException>(exception);
        }

        [Fact]
        public void CanExecute_WithoutPredicate_IsTrue()
        {
            // Given: a command without a predicate
            var command = new AsyncRelayCommand(_ => Task.CompletedTask);

            // When: the enabled state is read
            var canExecute = command.CanExecute(null);

            // Then: it can always be executed
            Assert.True(canExecute);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CanExecute_WithPredicate_ReturnsWhatThePredicateSays(bool answer)
        {
            // Given: a command whose predicate has a fixed answer
            var command = new AsyncRelayCommand(_ => Task.CompletedTask, _ => answer);

            // When: the enabled state is read
            var canExecute = command.CanExecute(null);

            // Then: it is the predicate's answer
            Assert.Equal(answer, canExecute);
        }

        [Fact]
        public void Constructor_GivenNoDelegate_Throws()
        {
            // Given: no delegate to run
            Func<object?, Task> execute = null!;

            // When: a command is created with it
            var exception = Record.Exception(() => new AsyncRelayCommand(execute));

            // Then: that is rejected right away
            Assert.IsType<ArgumentNullException>(exception);
        }
    }
}
