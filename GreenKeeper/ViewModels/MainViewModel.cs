using GreenKeeper.Models;
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

        // Selected plant in the ListView (Currently only dummy entries)
        private Plant? _SelectedPlant;
        public Plant? SelectedPlant
        {
            get { return _SelectedPlant; }
            set { _SelectedPlant = value; OnPropertyChanged(nameof(SelectedPlant)); }
        }

        // Implementation of INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
