using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IPlantRepository _plantRepository;
        private ObservableCollection<Plant> _plants;

        // Set all plants for the ListView
        public MainViewModel(IPlantRepository plantRepository)
        {
            _plantRepository = plantRepository;
            _plants = new ObservableCollection<Plant>(_plantRepository.GetPlants());
        }

        // All available plants for the ListView (currently only dummy entries)
        public ObservableCollection<Plant> Plants
        {
            get { return _plants; }
            set { _plants = value; OnPropertyChanged(nameof(Plants)); }
        }

        // Calculate the days until the the next watering
        public int? DaysUntilNextWatering
        {
            get
            {
                var waterSchedule = SelectedPlant?.CareSchedules
                    .FirstOrDefault(s => s.Care == CareType.Water);

                if (waterSchedule?.NextDueAt == null)
                {
                    return null;
                }

                // The days are the difference between the next due date and the current date

                return (int)Math.Ceiling(
                    (waterSchedule.NextDueAt.Value - DateTime.Now).TotalDays);
            }
        }

        // Selected plant in the ListView (Currently only dummy entries)
        private Plant? _SelectedPlant;
        public Plant? SelectedPlant
        {
            get { return _SelectedPlant; }
            set
            {
                _SelectedPlant = value;
                OnPropertyChanged(nameof(SelectedPlant));
                OnPropertyChanged(nameof(DaysUntilNextWatering));
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
