using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
