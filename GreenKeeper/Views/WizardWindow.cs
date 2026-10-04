using GreenKeeper.ViewModels.Wizards.Base;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Threading;

namespace GreenKeeper.Views
{
    /// <summary>
    /// Base of the wizard windows. A wizard swaps its step in place, and whatever had
    /// the focus in the old step is gone with it. A step with a field gets the focus
    /// there; one without (the summary) hands it to the default button.
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
                Dispatcher.BeginInvoke(DispatcherPriority.Input, FocusNewStep);
            }
        }

        private void FocusNewStep()
        {
            if (TryFocusFirstInput())
            {
                return;
            }

            VisualTree.Descendants<Button>(this)
                .FirstOrDefault(button => button.IsDefault && button.IsVisible && button.IsEnabled)
                ?.Focus();
        }
    }
}
