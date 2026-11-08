using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Threading;

namespace GreenKeeper.Views
{
    /// <summary>
    /// Tells a screen reader what is otherwise only visible: the result of an action,
    /// or a text that appears while the focus stays where it is.
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

            // A newer announcement replaces one that still waits. A screen reader can also cut
            // off what it is speaking at that moment, such as the name of a newly focused control.
            peer?.RaiseNotificationEvent(
                AutomationNotificationKind.ActionCompleted,
                AutomationNotificationProcessing.MostRecent,
                text,
                activityId: "GreenKeeper");
        }

        /// <summary>
        /// Announces the text once the screen has settled: after the layout and every
        /// pending focus change, so no focus change cuts it off.
        /// </summary>
        public static void AnnounceAfterFocusSettles(UIElement source, string text)
        {
            source.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, () => Announce(source, text));
        }

        private static void OnMessageChanged(DependencyObject element, DependencyPropertyChangedEventArgs e)
        {
            // Not while the element is being built: only a change the user caused is announced.
            if (element is FrameworkElement { IsLoaded: true } source)
            {
                Announce(source, (string?)e.NewValue ?? string.Empty);
            }
        }
    }
}
