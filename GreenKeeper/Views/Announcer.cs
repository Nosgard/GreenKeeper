using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Threading;

namespace GreenKeeper.Views
{
    /// <summary>
    /// Tells a screen reader what is otherwise only visible: the result of an action,
    /// or a text that appears while the focus stays where it is. Sighted users notice
    /// nothing of it.
    /// </summary>
    public static class Announcer
    {
        /// <summary>
        /// A text announced whenever it changes to something non-empty, for a message that
        /// shows up next to a field while the user types in it.
        /// </summary>
        public static readonly DependencyProperty MessageProperty = DependencyProperty.RegisterAttached(
            "Message",
            typeof(string),
            typeof(Announcer),
            new PropertyMetadata(string.Empty, OnMessageChanged));

        public static string GetMessage(UIElement element)
        {
            return (string)element.GetValue(MessageProperty);
        }

        public static void SetMessage(UIElement element, string message)
        {
            element.SetValue(MessageProperty, message);
        }

        public static void Announce(UIElement source, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var peer = UIElementAutomationPeer.FromElement(source) ?? UIElementAutomationPeer.CreatePeerForElement(source);

            // MostRecent: a newer announcement replaces one that is still waiting, and
            // neither cuts off what the screen reader is saying right now.
            peer?.RaiseNotificationEvent(
                AutomationNotificationKind.ActionCompleted,
                AutomationNotificationProcessing.MostRecent,
                text,
                activityId: "GreenKeeper");
        }

        /// <summary>
        /// Announces the text once the screen has settled: after the layout and every
        /// pending focus change, because a screen reader drops what it is saying when
        /// the focus moves.
        /// </summary>
        public static void AnnounceAfterFocusSettles(UIElement source, string text)
        {
            source.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, () => Announce(source, text));
        }

        private static void OnMessageChanged(DependencyObject element, DependencyPropertyChangedEventArgs e)
        {
            // Not while the element is being built: only a change the user caused is news.
            if (element is FrameworkElement { IsLoaded: true } source)
            {
                Announce(source, (string?)e.NewValue ?? string.Empty);
            }
        }
    }
}
