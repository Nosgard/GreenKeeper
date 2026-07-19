using GreenKeeper.Converters;
using GreenKeeper.Models.Enums;
using GreenKeeper.ViewModels.Base;
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
    public class EditActiveScheduleViewModel : AmountAndUnitInputViewModel
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
        }

        protected override void OnAmountOrUnitChanged()
        {
            OnPropertyChanged(nameof(PreviewText));
        }

        /// <summary>
        /// Shows the next expected due date.
        /// This is important so that the user knows, that the countdown is NOW running.
        /// The calculation of the next due date takes place NOW and not at the old due date
        /// </summary>
        public string PreviewText => HasValidAmount
            ? $"New due date: {TimeUnitConverter.ToDueDateText(TimeUnitConverter.ToDueDate(DateTime.Now, int.Parse(AmountText), SelectedUnit))}"
            : string.Empty;
    }
}
