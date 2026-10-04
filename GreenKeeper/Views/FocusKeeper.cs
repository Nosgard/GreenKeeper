using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.ViewModels.CareStatuses.Abstract;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace GreenKeeper.Views
{
    /// <summary>
    /// Keeps the keyboard focus where the user was working in the main window. The
    /// status cards and the plant entries are rebuilt when their data changes
    /// (Complete, Remove, a changed status in the periodic refresh), and a button can
    /// become disabled under the focus (Delete Plant). The focus then falls back to the
    /// window, and a keyboard or screen reader user would have to start over from the
    /// top. This class remembers what had the focus and puts it on the counterpart in
    /// the rebuilt screen, or on the nearest element that is still there.
    /// </summary>
    public sealed class FocusKeeper
    {
        private readonly Window _window;
        private readonly ListView _plantList;
        private readonly ItemsControl _careStatusList;

        // What had the focus last, and what it stood for.
        private UIElement? _lastElement;
        private Plant? _lastPlant;
        private (CareType Care, int CardIndex, object? Label)? _lastCardButton;

        private FocusKeeper(Window window, ListView plantList, ItemsControl careStatusList)
        {
            _window = window;
            _plantList = plantList;
            _careStatusList = careStatusList;
        }

        /// <summary>
        /// Keeps the focus in place in the window from now on. The window's focus
        /// event holds the keeper, so it lives as long as the window.
        /// </summary>
        public static void Attach(Window window, ListView plantList, ItemsControl careStatusList)
        {
            var keeper = new FocusKeeper(window, plantList, careStatusList);
            window.GotKeyboardFocus += keeper.Window_GotKeyboardFocus;
        }

        private void Window_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!IsFallback(e.NewFocus))
            {
                Remember(e.NewFocus as UIElement);
                return;
            }

            if (_lastElement != null && !IsUsable(_lastElement))
            {
                // Only once the rebuilt cards and entries exist.
                _window.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, Restore);
            }
        }

        // The window and the list itself are where the focus lands when its element is gone.
        private bool IsFallback(IInputElement? focus)
        {
            return ReferenceEquals(focus, _window) || ReferenceEquals(focus, _plantList);
        }

        private void Remember(UIElement? element)
        {
            _lastElement = element;
            _lastPlant = (element as ListViewItem)?.DataContext as Plant;
            _lastCardButton = element is Button { DataContext: CareStatusViewModel card } button
                ? (card.Care, _careStatusList.Items.IndexOf(card), button.Content)
                : null;
        }

        private void Restore()
        {
            // The user has moved on in the meantime.
            if (!IsFallback(Keyboard.FocusedElement))
            {
                return;
            }

            UIElement target = CardButton() ?? PlantEntry();
            target.Focus();
        }

        // The same button of the same card; if it is gone or disabled, the first button
        // of the card; if the card is gone, the first button of the card that took its
        // place, never its "Remove" just because the removed card's "Remove" had the focus.
        private Button? CardButton()
        {
            if (_lastCardButton is not { } lastCardButton)
            {
                return null;
            }

            var (care, cardIndex, label) = lastCardButton;
            var cards = _careStatusList.Items.OfType<CareStatusViewModel>().ToList();

            if (cards.Count == 0)
            {
                return null;
            }

            var card = cards.FirstOrDefault(candidate => candidate.Care == care)
                ?? cards[Math.Min(cardIndex, cards.Count - 1)];

            if (_careStatusList.ItemContainerGenerator.ContainerFromItem(card) is not { } container)
            {
                return null;
            }

            var buttons = VisualTree.Descendants<Button>(container).Where(IsUsable).ToList();

            var sameButton = card.Care == care ? buttons.FirstOrDefault(button => Equals(button.Content, label)) : null;

            return sameButton ?? buttons.FirstOrDefault();
        }

        // The entry of that plant, or the list itself while it has no entries.
        private UIElement PlantEntry()
        {
            object? plant = PlantToFocus();

            if (plant == null)
            {
                return _plantList;
            }

            // The list only builds the entries in view.
            _plantList.ScrollIntoView(plant);
            _plantList.UpdateLayout();

            return _plantList.ItemContainerGenerator.ContainerFromItem(plant) as UIElement ?? _plantList;
        }

        // The plant whose entry had the focus; without one, the selected or the first plant.
        private object? PlantToFocus()
        {
            if (_lastPlant != null && _plantList.Items.Contains(_lastPlant))
            {
                return _lastPlant;
            }

            return _plantList.SelectedItem ?? _plantList.Items.Cast<object>().FirstOrDefault();
        }

        private static bool IsUsable(UIElement element)
        {
            return element.IsVisible && element.IsEnabled && PresentationSource.FromVisual(element) != null;
        }
    }
}
