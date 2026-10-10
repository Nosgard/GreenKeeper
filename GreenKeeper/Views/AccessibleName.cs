using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;

namespace GreenKeeper.Views
{
    /// <summary>
    /// The name a screen reader announces for an element whose name changes while it
    /// is in use, like the theme toggle, which offers the other theme after a click.
    /// Sets AutomationProperties.Name and reports every change of it, which WPF does
    /// not do by itself; a screen reader on the element would otherwise keep the old
    /// name.
    /// </summary>
    public static class AccessibleName
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached(
            "Text",
            typeof(string),
            typeof(AccessibleName),
            new PropertyMetadata(string.Empty, OnTextChanged));

        public static string GetText(UIElement element)
        {
            return (string)element.GetValue(TextProperty);
        }

        public static void SetText(UIElement element, string text)
        {
            element.SetValue(TextProperty, text);
        }

        private static void OnTextChanged(DependencyObject element, DependencyPropertyChangedEventArgs e)
        {
            string oldName = (string?)e.OldValue ?? string.Empty;
            string newName = (string?)e.NewValue ?? string.Empty;
            AutomationProperties.SetName(element, newName);

            // A peer exists only once a screen reader or another client has asked for the element.
            if (element is UIElement uiElement
                && AutomationPeer.ListenerExists(AutomationEvents.PropertyChanged)
                && UIElementAutomationPeer.FromElement(uiElement) is { } peer)
            {
                peer.RaisePropertyChangedEvent(AutomationElementIdentifiers.NameProperty, oldName, newName);
            }
        }
    }
}
