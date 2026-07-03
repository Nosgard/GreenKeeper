using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.ViewModels.CareStatuses.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.ViewModels.CareStatuses
{
    public class ActiveCareStatusViewModel : CareStatusViewModel
    {
        private readonly CareSchedule? _schedule;

        protected ActiveCareStatusViewModel(CareType care, CareSchedule? schedule, string title, string iconSource, string iconBackgroundHex)
            : base(care, title, iconSource, iconBackgroundHex)
        {
            _schedule = schedule;
        }

        // Calculate the next due date in days. The amount of days will be depicted in the card of the care status
        public int? DaysUntilDue
        {
            get
            {
                if (_schedule?.NextDueAt == null)
                {
                    return null;
                }
                return (int)Math.Ceiling((_schedule.NextDueAt.Value - DateTime.Now).TotalDays);
            }
        }

        // Depending on the left days, set a specific text
        public override string StatusText
        {
            get
            {
                var days = DaysUntilDue;

                if (days == null)
                {
                    return string.Empty;
                }

                if (days == 0)
                {
                    return "Today";
                }

                if (days < 0)
                {
                    return $"Overdue for {Math.Abs(days.Value)} day{(Math.Abs(days.Value) > 1 ? "s" : "")}";
                }

                return $"Due in {days} day{(days > 1 ? "s" : "")}";
            }
        }
    }
}
