using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.Repositories;
using GreenKeeper.ViewModels.CareStatuses;
using GreenKeeper.ViewModels.CareStatuses.Abstract;
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

        // One card per defined CareType. The view models of each CareType contain their own metadata
        // For more information, go to: GreenKeeper/ViewModels/CareStatuses
        public IEnumerable<CareStatusViewModel> CareStatuses
        {
            get
            {
                if (SelectedPlant == null)
                {
                    yield break;
                }

                CareSchedule? ScheduleFor(CareType type) =>
                    SelectedPlant.CareSchedules.FirstOrDefault(s => s.Care == type);

                yield return new WateringStatusViewModel(ScheduleFor(CareType.Water));
                yield return new FertilizingStatusViewModel(ScheduleFor(CareType.Nutrients));
                yield return new SunlightStatusViewModel(SelectedPlant.SunlightRequirement);
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
