using GreenKeeper.Commands;
using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.Repositories;
using GreenKeeper.Services;
using GreenKeeper.ViewModels.CareStatuses;
using GreenKeeper.ViewModels.CareStatuses.Abstract;
using GreenKeeper.ViewModels.CareStatuses.Active;
using GreenKeeper.ViewModels.CareStatuses.Passive;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GreenKeeper.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IPlantRepository _plantRepository;
        private readonly IDialogService _dialogService;
        private ObservableCollection<Plant> _plants;

        // Set all plants for the ListView
        /// <summary>
        /// Important notes for OpenNotesCommand:
        /// Commands always need to be initialized in the constructor.
        /// The OpenNotesCommand is get-only, which means that if there
        /// is no allocation it remains 'null', the compiler will
        /// cause no alarm and WPF ignores the click on a bound command
        /// set to 'null'.
        /// 
        /// execute: Rules what happens after a click.
        /// You only fire an event (OpenNotesRequested), instead of opening
        /// a new window directly.
        /// -> Why? Because the ViewModel should not know anything about the view
        /// 
        /// canExecute: Controls the bound button whether to be enabled or disabled.
        /// If there is no plant selected, the button remains deactivated.
        /// </summary>
        /// <param name="plantRepository"></param>
        public MainViewModel(IPlantRepository plantRepository, IDialogService dialogService)
        {
            _plantRepository = plantRepository;
            _dialogService = dialogService;
            _plants = new ObservableCollection<Plant>(_plantRepository.GetPlants());

            // Add Plant Wizard related Command
            AddPlantCommand = new RelayCommand(
                execute: _ => AddPlantRequested?.Invoke(this, EventArgs.Empty));

            AddScheduleCommand = new RelayCommand(
                execute: _ => AddScheduleRequested?.Invoke(this, SelectedPlant!),
                canExecute: _ => SelectedPlant != null);

            // Delete Plant Button related Command
            DeletePlantCommand = new RelayCommand(
                execute: _ => DeleteSelectedPlant(),
                canExecute: _ => SelectedPlant != null);

            // Notes related Command
            OpenNotesCommand = new RelayCommand(
                execute: _ => OpenNotesRequested?.Invoke(this, SelectedPlant!),
                canExecute: _ => SelectedPlant != null);
        }

        // All available plants for the ListView (currently only dummy entries)
        public ObservableCollection<Plant> Plants
        {
            get { return _plants; }
            set { _plants = value; OnPropertyChanged(nameof(Plants)); }
        }

        /// <summary>
        /// One card per defined CareType. Depending on the set care types,
        /// only the cards of the allocated care types will appear in the status.
        /// 
        /// Important!
        /// The watering schedule is mandatory for all plants, which means that
        /// it's status card is always visible.
        /// The fertilizing schedule and the separate sunlight requirement are optional
        /// and will not show up if they are not set to a plant
        /// </summary>
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

                // The watering schedule is mandatory for every plant
                yield return new WateringStatusViewModel(ScheduleFor(CareType.Water));

                // The fertilizing schedule is optional (only show the status if set to a plant)
                var fertilizingSchedule = ScheduleFor(CareType.Nutrients);
                if (fertilizingSchedule != null)
                {
                    yield return new FertilizingStatusViewModel(ScheduleFor(CareType.Nutrients));
                }

                // The sunlight requirement is optional (only show the status if set to a plant)
                if (SelectedPlant.SunlightRequirement != null)
                {
                    yield return new SunlightStatusViewModel(SelectedPlant.SunlightRequirement);
                }
            }
        }

        // Selected plant in the ListView (Currently only dummy entries)
        /// <summary>
        /// Reminder:
        /// An explicit "Requery" of OpenNotesCommand is not necessary.
        /// RelayCommand is hanging on CommandManager.RequerySuggested,
        /// which automatically requests CanExecute on most UI-interactions
        /// </summary>
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

        // Essential command to be bound to the Notes-Button in MainWIndow.xaml.
        // It is ICommand so that the View only binds the interface and
        // the explicit implementation remains exchangeable
        public ICommand OpenNotesCommand { get; }

        // Notify the View, that a new Notes-Window for the given plant should be opened.
        // Will be subscribed by MainWindow.xaml.cs (for more information, go there).
        // Code-Behind opens the window (View), while the ViewModel does not know any Window-Class
        public event EventHandler<Plant>? OpenNotesRequested;


        // Add Plant Wizard Section
        public ICommand AddPlantCommand { get; }
        public event EventHandler? AddPlantRequested;

        // Add a new plant to the ListView that was created in the Wizard.
        // No manual OnPropertyChanged needed because it's being added to the ObservableCollection
        public void AddPlant(Plant plant)
        {
            Plants.Add(plant);
        }

        // Add Schedule Wizard Section
        public ICommand AddScheduleCommand { get; }
        public event EventHandler<Plant>? AddScheduleRequested;

        // Delete Plant Button Section
        public ICommand DeletePlantCommand { get; }

        private void DeleteSelectedPlant()
        {
            if (SelectedPlant == null)
            {
                return;
            }

            bool isConfirmed = _dialogService.Confirm(
                $"Are you sure you want to delete \"{SelectedPlant.Name}\"? This cannot be undone.",
                "Delete Plant");

            if (!isConfirmed)
            {
                return;
            }

            Plants.Remove(SelectedPlant);

            // After removing a plant, there is no "selected" plant.
            // Prevent the Dashboard from presenting non-existing data
            SelectedPlant = null;
        }

        /// <summary>
        /// Will be called after the AddScheduleWizard successfully applied a Care Schedule / Sunlight Requirement
        /// on the selected Plant-Object. Because the Change doesn't apply via the ObservableCollection
        /// (Plant was changed, not swapped) the UI doesn't need to be notified to show the new Status-Card
        /// </summary>
        public void RefreshCareStatuses()
        {
            OnPropertyChanged(nameof(CareStatuses));
        }

        // Implementation of INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
