using GreenKeeper.ViewModels.Wizards.Base;
using System.ComponentModel;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Threading;

namespace GreenKeeper.Views
{
    /// <summary>
    /// Base of the wizard windows. A wizard swaps its step in place, and whatever had
    /// the focus in the old step is gone with it. A step with a field gets the focus
    /// there; one without (the summary) hands it to the default button and is read to
    /// a screen reader, which would otherwise say nothing about it.
    /// </summary>
    public class WizardWindow : DialogWindow
    {
        protected void Attach(WizardViewModel wizard)
        {
            base.Attach(wizard);
            wizard.PropertyChanged += Wizard_PropertyChanged;
        }

        // The fields of a new step only exist once the step is laid out, so the focus
        // waits on the dispatcher.
        private void Wizard_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(WizardViewModel.CurrentStep))
            {
                var step = ((WizardViewModel)sender!).CurrentStep;
                Dispatcher.BeginInvoke(DispatcherPriority.Input, () => FocusNewStep(step));
            }
        }

        // A step with a field is announced by the field's name once it has the focus.
        private void FocusNewStep(IWizardStepViewModel step)
        {
            if (TryFocusFirstInput())
            {
                return;
            }

            VisualTree.Descendants<Button>(this)
                .FirstOrDefault(button => button.IsDefault && button.IsVisible && button.IsEnabled)
                ?.Focus();

            Announcer.AnnounceAfterFocusSettles(this, TextOf(step));
        }

        // The visible texts of the step, in their spoken form where they have one. Only
        // those of the step itself: the buttons belong to the window around it.
        private string TextOf(IWizardStepViewModel step)
        {
            var stepHost = VisualTree.Descendants<ContentPresenter>(this)
                .FirstOrDefault(presenter => ReferenceEquals(presenter.Content, step));

            if (stepHost == null)
            {
                return string.Empty;
            }

            var texts = VisualTree.Descendants<TextBlock>(stepHost)
                .Where(text => text.IsVisible && text.Text.Length > 0)
                .Select(text => AutomationProperties.GetName(text) is { Length: > 0 } name ? name : text.Text);

            return string.Join(". ", texts);
        }
    }
}
