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

        // Metadata per CareType (Title, Icon, Icon-Background-Color)
        // Sunlighting is missing
        private static readonly Dictionary<CareType, (string Title, string Icon, string Color)> CareTypeMeta = new()
        {
            { CareType.Water, ("Watering", "/Resources/Icons/Waterdrop.png", "#4accff") },
            { CareType.Nutrients, ("Fertilizing", "/Resources/Icons/Pill.png", "#ff695b") }
        };

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

        // One card per defined CareType, filled with the appropriate schedule (as long as it exists)
        public IEnumerable<CareStatusViewModel> CareStatuses
        {
            get
            {
                if (SelectedPlant == null)
                {
                    yield break;
                }

                foreach (var (careType, meta) in CareTypeMeta)
                {
                    var schedule = SelectedPlant.CareSchedules
                        .FirstOrDefault(s => s.Care == careType);

                    yield return new CareStatusViewModel(careType, schedule, meta.Title, meta.Icon, meta.Color);
                }
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
                OnPropertyChanged(nameof(CareStatuses));
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
