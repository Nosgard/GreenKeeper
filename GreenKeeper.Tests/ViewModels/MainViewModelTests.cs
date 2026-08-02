using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.Tests.Fakes;
using GreenKeeper.ViewModels;
using GreenKeeper.ViewModels.CareStatuses.Active;
using GreenKeeper.ViewModels.CareStatuses.Passive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.Tests.ViewModels
{
    public class MainViewModelTests
    {
        [Fact]
        public async Task InitializeAsync_GivenRepositoryWithOnePlant_PopulatesPlants()
        {
            // Given: a repository seeded with one existing plant
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(new Models.Plant { Name = "Aloe Vera" });

            var dialogService = new FakeDialogService();
            var timerService = new FakeTimerService();

            var viewModel = new MainViewModel(plantRepository, dialogService, timerService);

            // When: InitializeAsync is called
            await viewModel.InitializeAsync();

            // Then: Plants should contain exactly the seeded plant
            Assert.Single(viewModel.Plants);
            Assert.Equal("Aloe Vera", viewModel.Plants[0].Name);
        }

        [Fact]
        public async Task AddPlantAsync_GivenExistingPlants_AppendsWithoutRemovingExisting()
        {
            // Given: a repository already containing one plant
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(new Models.Plant { Name = "Aloe Vera" });

            var dialogService = new FakeDialogService();
            var timerService = new FakeTimerService();

            var viewModel = new MainViewModel(plantRepository, dialogService, timerService);
            await viewModel.InitializeAsync();

            // When: a second plant is added
            await viewModel.AddPlantAsync(new Models.Plant { Name = "Basil" });

            // Then: both plants should be present, not just the new one
            Assert.Equal(2, viewModel.Plants.Count);
            Assert.Contains(viewModel.Plants, p => p.Name == "Aloe Vera");
            Assert.Contains(viewModel.Plants, p => p.Name == "Basil");
        }

        [Fact]
        public async Task AddPlantAsync_GivenRepositoryThrows_PropagatesExceptionAndDoesNotAddPlant()
        {
            // Given: a repository configured to fail when saving
            var plantRepository = new FakePlantRepository { ShouldThrowOnAdd = true };
            var dialogService = new FakeDialogService();
            var timerService = new FakeTimerService();

            var viewModel = new MainViewModel(plantRepository, dialogService, timerService);

            // When: InitializeAsync is called
            await viewModel.InitializeAsync();

            // Then: adding a plant should propagate the exception and the plant should NOT appear in the UI-bound collection
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => viewModel.AddPlantAsync(new Models.Plant { Name = "Basil" }));

            Assert.Empty(viewModel.Plants);
        }

        [Fact]
        public async Task DeletePlantCommand_GivenUserConfirms_RemovesPlantFromRepositoryAndCollection()
        {
            // Given: a repository with one plant, currently selected, and the
            // dialog service configured to simulate the user choosing "Yes"
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(new Models.Plant { Name = "Aloe Vera" });

            var dialogService = new FakeDialogService { ConfirmResult = true };
            var timerService = new FakeTimerService();

            var viewModel = new MainViewModel(plantRepository, dialogService, timerService);
            await viewModel.InitializeAsync();
            viewModel.SelectedPlant = viewModel.Plants[0];

            // When: DeletePlantCommand is executed
            viewModel.DeletePlantCommand.Execute(null);

            // Then: the plant is gone from both the UI collection and the repository and no plant remains selected
            Assert.Empty(viewModel.Plants);
            Assert.Empty(await plantRepository.GetPlantsAsync());
            Assert.Null(viewModel.SelectedPlant);
        }

        [Fact]
        public async Task DeletePlantCommand_GivenUserDeclines_KeepsPlantUnchanged()
        {
            // Given: a repository with one plant, currently selected, and the
            // dialog service configured to simulate the user choosing "No"
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(new Models.Plant { Name = "Aloe Vera" });

            var dialogService = new FakeDialogService { ConfirmResult = false };
            var timerService = new FakeTimerService();

            var viewModel = new MainViewModel(plantRepository, dialogService, timerService);
            await viewModel.InitializeAsync();
            var selectedPlant = viewModel.Plants[0];
            viewModel.SelectedPlant = selectedPlant;

            // When: DeletePlantCommand is executed
            viewModel.DeletePlantCommand.Execute(null);

            // Then: nothing changed - the plant remains in both the collection and the repository, and stays selected
            Assert.Single(viewModel.Plants);
            Assert.Single(await plantRepository.GetPlantsAsync());
            Assert.Equal(selectedPlant, viewModel.SelectedPlant);
        }

        [Fact]
        public async Task DeletePlantCommand_GivenRepositoryThrows_ShowsErrorAndKeepsPlant()
        {
            // Given: a repository configured to fail on delete, one plant selected, and the user confirming the deletion
            var plantRepository = new FakePlantRepository { ShouldThrowOnDelete = true };
            plantRepository.SeedPlants(new Models.Plant { Name = "Aloe Vera" });

            var dialogService = new FakeDialogService { ConfirmResult = true };
            var timerService = new FakeTimerService();

            var viewModel = new MainViewModel(plantRepository, dialogService, timerService);
            await viewModel.InitializeAsync();
            var selectedPlant = viewModel.Plants[0];
            viewModel.SelectedPlant = selectedPlant;

            // When: DeletePlantCommand is executed
            viewModel.DeletePlantCommand.Execute(null);

            // Then: an error is shown, and the plant stays exactly as it was - still in the UI collection and still selected,
            // since the deletion never actually succeeded
            Assert.True(dialogService.ShowErrorWasCalled);
            Assert.Single(viewModel.Plants);
            Assert.Equal(selectedPlant, viewModel.SelectedPlant);
        }

        [Fact]
        public async Task SelectedPlant_GivenPlantIsSelected_UpdatesIsPlantSelectedAndRaisesPropertyChanged()
        {
            // Given: an initialized MainViewModel with one plant, nothing selected yet
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(new Models.Plant { Name = "Aloe Vera" });

            var dialogService = new FakeDialogService();
            var timerService = new FakeTimerService();

            var viewModel = new MainViewModel(plantRepository, dialogService, timerService);
            await viewModel.InitializeAsync();

            // Sanity check on the initial state, before the actual "When" happens
            Assert.False(viewModel.IsPlantSelected);

            var raisedProperties = new List<string>();
            viewModel.PropertyChanged += (_, e) => raisedProperties.Add(e.PropertyName!);

            // When: a plant is selected
            viewModel.SelectedPlant = viewModel.Plants[0];

            // Then: IsPlantSelected reflects the new state, and PropertyChanged was
            // raised for all three properties that depend on the selection
            Assert.True(viewModel.IsPlantSelected);
            Assert.Contains(nameof(viewModel.SelectedPlant), raisedProperties);
            Assert.Contains(nameof(viewModel.IsPlantSelected), raisedProperties);
            Assert.Contains(nameof(MainViewModel.IsPlantSelected), raisedProperties);
        }

        [Fact]
        public async Task SearchText_GivenPlantIsSelected_ResetsSelectedPlantToNull()
        {
            // Given: an initialized MainViewModel with a plant currently selected
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(new Models.Plant { Name = "Aloe Vera" });

            var dialogService = new FakeDialogService();
            var timerService = new FakeTimerService();

            var viewModel = new MainViewModel(plantRepository, dialogService, timerService);
            await viewModel.InitializeAsync();
            viewModel.SelectedPlant = viewModel.Plants[0];

            // Sanity check before the actual "When"
            Assert.NotNull(viewModel.SelectedPlant);

            // When: the search text changes
            viewModel.SearchText = "al";

            // Then: the previously selected plant is deselected
            Assert.Null(viewModel.SelectedPlant);
        }

        [Fact]
        public async Task SearchText_GivenSameValueIsSetAgain_DoesNotResetSelectedPlant()
        {
            // Given: an initialized MainViewModel with a plant selected, and
            // SearchText already set to a specific value
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(new Models.Plant { Name = "Aloe Vera" });

            var dialogService = new FakeDialogService();
            var timerService = new FakeTimerService();

            var viewModel = new MainViewModel(plantRepository, dialogService, timerService);
            await viewModel.InitializeAsync();
            viewModel.SearchText = "al";
            viewModel.SelectedPlant = viewModel.Plants[0];

            // When: SearchText is set to the exact same value again
            viewModel.SearchText = "al";

            // Then: nothing actually changed, so the selection should be preserved
            // (the setter's early-return guard for unchanged values should prevent
            // the deselection logic from running again)
            Assert.NotNull(viewModel.SelectedPlant);
        }

        // -- Care-Statuses Tests --

        [Fact]
        public async Task CareStatuses_GivenPlantWithOnlyWatering_ReturnsOnlyWateringCard()
        {
            // Given: a plant with only a Care-Schedule for Watering, no Fertilizing, no Sunlight
            var plant = new Plant { Name = "Cactus" };
            plant.CareSchedules.Add(new CareSchedule { Care = CareType.Water, IntervalAmount = 7, IntervalUnit = TimeUnit.Days });

            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var dialogService = new FakeDialogService();
            var timerService = new FakeTimerService();

            var viewModel = new MainViewModel(plantRepository, dialogService, timerService);
            await viewModel.InitializeAsync();
            viewModel.SelectedPlant = viewModel.Plants[0];

            // When: Care-Statuses is read
            var careStatuses = viewModel.CareStatuses.ToList();

            // Then: exactly one card for Watering
            Assert.Single(careStatuses);
            Assert.IsType<WateringStatusViewModel>(careStatuses[0]);
        }

        [Fact]
        public async Task CareStatuses_GivenPlantWithAllCareTypes_ReturnsAllThreeStatusCardsInOrder()
        {
            // Given: a plant with Watering, Fertilizing and SunlightRequirement all set
            var plant = new Plant { Name = "Aloe Vera" };
            plant.CareSchedules.Add(new CareSchedule { Care = CareType.Water, IntervalAmount = 7, IntervalUnit = TimeUnit.Days });
            plant.CareSchedules.Add(new CareSchedule { Care = CareType.Nutrients, IntervalAmount = 30, IntervalUnit = TimeUnit.Days });
            plant.SunlightRequirement = new SunlightRequirement { Hours = 6, Period = SunlightPeriod.Day };

            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var dialogService = new FakeDialogService();
            var timerService = new FakeTimerService();

            var viewModel = new MainViewModel(plantRepository, dialogService, timerService);
            await viewModel.InitializeAsync();
            viewModel.SelectedPlant = viewModel.Plants[0];

            // When: Care-Statuses is read
            var careStatuses = viewModel.CareStatuses.ToList();

            // Then: all three cards are present, in the expected order
            Assert.Equal(3, careStatuses.Count);
            Assert.IsType<WateringStatusViewModel>(careStatuses[0]);
            Assert.IsType<FertilizingStatusViewModel>(careStatuses[1]);
            Assert.IsType<SunlightStatusViewModel>(careStatuses[2]);
        }

        [Fact]
        public async Task CareStatuses_GivenPlantWithWateringAndSUnlightButNoFertilizing_ReturnsOnlyThoseTwoStatusCards()
        {
            // Given: a plant with Watering and SunlightRequirement, but no Fertilizing
            var plant = new Plant { Name = "Snake Plant" };
            plant.CareSchedules.Add(new CareSchedule { Care = CareType.Water, IntervalAmount = 14, IntervalUnit = TimeUnit.Days });
            plant.SunlightRequirement = new SunlightRequirement { Hours = 4, Period = SunlightPeriod.Day };

            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var dialogService = new FakeDialogService();
            var timerService = new FakeTimerService();

            var viewModel = new MainViewModel(plantRepository, dialogService, timerService);
            await viewModel.InitializeAsync();
            viewModel.SelectedPlant = viewModel.Plants[0];

            // When: Care-Statuses is read
            var careStatuses = viewModel.CareStatuses.ToList();

            // Then: exactly Watering and SUnlight, no Fertilizing between them
            Assert.Equal(2, careStatuses.Count);
            Assert.IsType<WateringStatusViewModel>(careStatuses[0]);
            Assert.IsType<SunlightStatusViewModel>(careStatuses[1]);
        }
    }
}
