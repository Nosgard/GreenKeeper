using GreenKeeper.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.ViewModels.Notes
{
    public class NotesViewModel : INotifyPropertyChanged
    {
        private readonly Plant _plant;

        public NotesViewModel(Plant plant)
        {
            _plant = plant;
        }

        public string PlantName => _plant.Name;

        // Extract the Notes-property from the Plant-Model to present it in the related view
        // Currently only eligible for dummy-objects. Implementation for EF-Core will follow
        public string Notes
        {
            get => _plant.Notes ?? string.Empty;
            set
            {
                _plant.Notes = value;
                OnPropertyChanged(nameof(Notes));
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
