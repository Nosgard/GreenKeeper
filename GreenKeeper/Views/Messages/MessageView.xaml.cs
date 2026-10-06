using GreenKeeper.Services;
using GreenKeeper.ViewModels.Messages;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace GreenKeeper.Views.Messages
{
    /// <summary>
    /// Interaction logic for MessageView.xaml.
    /// </summary>
    public partial class MessageView : DialogWindow
    {
        // SC_CLOSE ("Close" in the menu of the title bar) and MF_GRAYED (the state that
        // greys an entry out), from winuser.h.
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

            // The focus starts on the answer that loses nothing: a hasty key gives that
            // answer, and the arrow keys lead to the other one.
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
        /// A question has to be answered with one of its buttons. Closing the window any
        /// other way would count as a "No" nobody gave, and for "Do you want to save?"
        /// that discards the changes. So the X of a question is greyed out, as a Windows
        /// message box shows it for Yes/No; Windows then ignores Alt+F4 as well.
        /// </summary>
        private void MessageView_SourceInitialized(object? sender, EventArgs e)
        {
            if (_isQuestion)
            {
                IntPtr titleBarMenu = GetSystemMenu(new WindowInteropHelper(this).Handle, revert: false);

                // Returns the entry's previous state, which does not matter here.
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
