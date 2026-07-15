using GreenKeeper.Converters;
using GreenKeeper.Models.Enums;
using GreenKeeper.ViewModels.Wizards.Base.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.ViewModels.CareStatuses.EditOption
{
    /// <summary>
    /// Base of the Edit-Option for all active statuses (Watering / Fertilizing).
    /// </summary>
    public class EditActiveScheduleViewModel : ActiveStepViewModel
    {
        public string Title { get; }

        public EditActiveScheduleViewModel(string title, int? initialAmount, TimeUnit initialUnit)
        {
            // Title of the Care-Type (Watering / Fertilizing)
            Title = title;

            // Fill the amount text with the original value, that was set in the Wizard beforehand
            if (initialAmount.HasValue)
            {
                AmountText = initialAmount.Value.ToString();
            }

            SelectedUnit = initialUnit;

            // Little trick: Instead of modifying ActiveStepViewModel itself, this ViewModel is listening
            // to it's own PropertyChanged event. Use this to keep PreviewText automatically in sync without touching the base class
            PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(AmountText) || e.PropertyName == nameof(SelectedUnit))
                {
                    OnPropertyChanged(nameof(PreviewText));
                }
            };
        }

        
        public override bool CanProceed => HasValidAmount;

        /// <summary>
        /// Shows the next expected due date.
        /// This is important so that the user knows, that the countdown is NOW running.
        /// The calculation of the next due date takes place NOW and not at the old due date
        /// </summary>
        public string PreviewText => HasValidAmount
            ? $"New due date: {TimeUnitConverter.ToDueDateText(DateTime.Now.Add(TimeUnitConverter.ToTimeSpan(int.Parse(AmountText), SelectedUnit)))}"
            : string.Empty;
    }
}
