using GreenKeeper.Services;
using GreenKeeper.ViewModels.Messages;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace GreenKeeper.Views.Messages
{
    public partial class MessageView : DialogWindow
    {
        private const uint CloseCommand = 0xF060;
        private const uint Grayed = 0x0001;

        private readonly bool _isQuestion;

        // False until one of the buttons has given the answer.
        private bool _isAnswered;

        public MessageView(string message, string title, bool isQuestion, Answer firstAnswer)
        {
            InitializeComponent();
            Title = title;
            _isQuestion = isQuestion;
            Attach(new MessageViewModel(message, isQuestion));

            // The focus starts on the answer that loses nothing, so Enter right away gives that
            // answer; the arrow keys lead to the other one.
            Loaded += (_, _) => (firstAnswer == Answer.No ? DeclineButton : ConfirmButton).Focus();
            SourceInitialized += MessageView_SourceInitialized;
            Closing += MessageView_Closing;
        }

        protected override void CloseWithResult(bool dialogResult)
        {
            _isAnswered = true;
            base.CloseWithResult(dialogResult);
        }

        /// <summary>
        /// A question has to be answered with one of its buttons: closing it any other way
        /// would count as a "No" nobody gave, which can discard changes. So its X is greyed
        /// out as in a Windows message box. Greying it out also turns off Alt+F4.
        /// </summary>
        private void MessageView_SourceInitialized(object? sender, EventArgs e)
        {
            if (_isQuestion)
            {
                IntPtr titleBarMenu = GetSystemMenu(new WindowInteropHelper(this).Handle, revert: false);

                _ = EnableMenuItem(titleBarMenu, CloseCommand, Grayed);
            }
        }

        // Refuses every request to close a question that no button has answered yet.
        private void MessageView_Closing(object? sender, CancelEventArgs e)
        {
            if (_isQuestion && !_isAnswered)
            {
                e.Cancel = true;
            }
        }

        [DllImport("user32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern IntPtr GetSystemMenu(IntPtr window, bool revert);

        [DllImport("user32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern int EnableMenuItem(IntPtr menu, uint item, uint state);
    }
}
