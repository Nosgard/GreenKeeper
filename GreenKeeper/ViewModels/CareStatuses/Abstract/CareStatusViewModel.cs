using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace GreenKeeper.ViewModels.CareStatuses.Abstract
{
    public abstract class CareStatusViewModel : INotifyPropertyChanged
    {
        private readonly CareSchedule? _schedule;

        public CareType Care { get; }
        public string Title { get; }
        public string IconSource { get; }
        public Brush IconBackground { get; }

        // Removes an optional status card (Fertilizing, Sunlight).
        // Nullable because mandatory status cards such as Watering are unremovable, so for them it is null.
        public ICommand? RemoveCommand { get; protected set; }

        // Allows the user to edit the value of a status card that was set beforehand.
        // This applies to all available status cards, but it is set by the concrete ViewModel of the status.
        public ICommand? EditCommand { get; protected set; }

        // Provide all important data for the card of the care status.
        public CareStatusViewModel(CareType care, string title, string iconSource, string iconBackgroundHex)
        {
            Care = care;
            Title = title;
            IconSource = iconSource;
            IconBackground = (Brush)new BrushConverter().ConvertFromString(iconBackgroundHex.ToString())!;
        }

        // All status cards implement the status text in their own way.
        public abstract string StatusText { get; }

        /// <summary>
        /// Whether the status text should read as overdue. Sits on the base class
        /// so the shared card template can bind it on every card - Sunlight has no
        /// due date and stays false.
        /// </summary>
        public virtual bool IsOverdue => false;

        // Implementation of INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
