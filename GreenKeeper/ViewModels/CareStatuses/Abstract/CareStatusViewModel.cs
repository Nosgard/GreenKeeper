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

        // Removes an optional Status-Card (Fertilizing, Sunlight).
        // Nullable because mandatory Status-Cards above all Watering are unremovable so they are null
        public ICommand? RemoveCommand { get; protected set; }

        // Provide all important data for the card of the care status
        public CareStatusViewModel(CareType care, string title, string iconSource, string iconBackgroundHex)
        {
            Care = care;
            Title = title;
            IconSource = iconSource;
            IconBackground = (Brush)new BrushConverter().ConvertFromString(iconBackgroundHex.ToString())!;
        }

        // All status cards implement the status text in their own way
        public abstract string StatusText { get; }

        // Implementation of INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
