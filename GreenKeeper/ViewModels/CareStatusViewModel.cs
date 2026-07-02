using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace GreenKeeper.ViewModels
{
    public class CareStatusViewModel : INotifyPropertyChanged
    {
        private readonly CareSchedule? _schedule;

        public CareType Care { get; }
        public string Title { get; }
        public string IconSource { get; }
        public Brush IconBackground { get; }
        public bool IsRemovable { get; }

        // Provide all important data for the card of the care status
        public CareStatusViewModel(CareType care, CareSchedule? schedule, string title, string iconSource, string iconBackgroundHex, bool isRemovable)
        {
            Care = care;
            _schedule = schedule;
            Title = title;
            IconSource = iconSource;
            IconBackground = (Brush)new BrushConverter().ConvertFromString(iconBackgroundHex.ToString())!;
            IsRemovable = isRemovable;
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

        // Implementation of INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
