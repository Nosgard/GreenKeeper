using GreenKeeper.Converters;
using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.ViewModels.CareStatuses.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GreenKeeper.ViewModels.CareStatuses.Active
{
    public class ActiveCareStatusViewModel : CareStatusViewModel
    {
        private readonly CareSchedule? _schedule;

        protected ActiveCareStatusViewModel(CareType care, CareSchedule? schedule, string title, string iconSource, string iconBackgroundHex)
            : base(care, title, iconSource, iconBackgroundHex)
        {
            _schedule = schedule;
        }

        // Checks if the Complete-Button can be used or not,
        // depending on whether the next due date is today, overdue or in the future
        public bool IsCompletable => _schedule?.NextDueAt != null && _schedule.NextDueAt.Value <= DateTime.Now;

        // Fires the Completion of the care,
        // once it's done, recalculate the new due date for NOW
        public ICommand? CompleteCommand { get; protected set; }

        // The whole logic for the conversion of the time units is being controlled by the TimeUnitConverter
        public override string StatusText =>
            TimeUnitConverter.ToDueDateText(_schedule?.NextDueAt);
    }
}
