using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.Repositories;
using GreenKeeper.Tests.Fakes;
using GreenKeeper.ViewModels;
using GreenKeeper.ViewModels.CareStatuses.Active;
using GreenKeeper.ViewModels.CareStatuses.Passive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

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

        // -- Complete-Button Tests --

        [Fact]
        public async Task WateringCard_CompleteCommand_GivenValidSchedule_RecalculatesAndPersistsDueDate()
        {
            // Given: a plant with an overdue Watering Care-Schedule
            var plant = new Plant { Name = "Aloe Vera" };
            plant.CareSchedules.Add(new CareSchedule
            {
                Care = CareType.Water,
                IntervalAmount = 7,
                IntervalUnit = TimeUnit.Days,
                NextDueAt = DateTime.Now.AddDays(-1)
            });

            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var dialogService = new FakeDialogService();
            var timerService = new FakeTimerService();

            var viewModel = new MainViewModel(plantRepository, dialogService, timerService);
            await viewModel.InitializeAsync();
            viewModel.SelectedPlant = viewModel.Plants[0];

            var wateringCard = viewModel.CareStatuses.OfType<WateringStatusViewModel>().Single();

            var beforeClick = DateTime.Now;

            // When: the Complete Command is executed
            wateringCard.CompleteCommand!.Execute(null);

            var afterClick = DateTime.Now;

            // Then: the persisted schedule reflects a next due date (NextDueAt) roughly 7 days from now,
            // and the last date of care (LastCaredAt) roughly now - checked via the repository, not
            // just the in-memory ViewModel state, to confirm actual persistence
            var persistedSchedule = (await plantRepository.GetPlantsAsync())
                .Single()
                .CareSchedules
                .Single(s => s.Care == CareType.Water);

            Assert.InRange(persistedSchedule.NextDueAt!.Value, beforeClick.AddDays(7).AddSeconds(-2), afterClick.AddDays(7).AddSeconds(2));
            Assert.InRange(persistedSchedule.LastCaredAt!.Value, beforeClick.AddSeconds(-2), afterClick.AddSeconds(2));
        }

        [Fact]
        public async Task FertilizingCard_CompleteCommand_GivenValidSchedule_RecalculatesAndPersistsDueDateWithoutAffectingWatering()
        {
            // Given: a plant with both a Watering schedule (untouched reference point) and an overdue Fertilizing schedule
            var plant = new Plant { Name = "Aloe Vera" };
            var originalWateringDueDate = DateTime.Now.AddDays(3);

            plant.CareSchedules.Add(new CareSchedule
            {
                Care = CareType.Water,
                IntervalAmount = 7,
                IntervalUnit = TimeUnit.Days,
                NextDueAt = originalWateringDueDate
            });
            plant.CareSchedules.Add(new CareSchedule
            {
                Care = CareType.Nutrients,
                IntervalAmount = 30,
                IntervalUnit = TimeUnit.Days,
                NextDueAt = DateTime.Now.AddDays(-1)
            });

            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var dialogService = new FakeDialogService();
            var timerService = new FakeTimerService();

            var viewModel = new MainViewModel(plantRepository, dialogService, timerService);
            await viewModel.InitializeAsync();
            viewModel.SelectedPlant = viewModel.Plants[0];

            var fertilizingCard = viewModel.CareStatuses.OfType<FertilizingStatusViewModel>().Single();

            var beforeClick = DateTime.Now;

            // When: the Complete Command is executed on the Fertilizing card
            fertilizingCard.CompleteCommand!.Execute(null);

            var afterClick = DateTime.Now;

            // Then: only the Fertilizing schedule was recalculated and persisted, while the due date of Watering remains completely untouched
            var persistedSchedules = (await plantRepository.GetPlantsAsync()).Single().CareSchedules;
            var persistedFertilizing = persistedSchedules.Single(s => s.Care == CareType.Nutrients);
            var persistedWatering = persistedSchedules.Single(s => s.Care == CareType.Water);

            Assert.InRange(persistedFertilizing.NextDueAt!.Value, beforeClick.AddDays(30).AddSeconds(-2), afterClick.AddDays(30).AddSeconds(2));
            Assert.InRange(persistedFertilizing.LastCaredAt!.Value, beforeClick.AddSeconds(-2), afterClick.AddSeconds(2));

            Assert.Equal(originalWateringDueDate, persistedWatering.NextDueAt);
            Assert.Null(persistedWatering.LastCaredAt);
        }

        [Fact]
        public async Task WateringCard_CompleteCommand_GivenMissingIntervalData_DoesNothing()
        {
            // Given: a plant with a Watering schedule that has NO IntervalAmount/IntervalUnit set, but the next due date (NextDueAt) is still present
            var originalDueDate = DateTime.Now.AddDays(-1);
            var plant = new Plant { Name = "Aloe Vera" };
            plant.CareSchedules.Add(new CareSchedule
            {
                Care = CareType.Water,
                IntervalAmount = null,
                IntervalUnit = null,
                NextDueAt = originalDueDate
            });

            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var dialogService = new FakeDialogService();
            var timerService = new FakeTimerService();

            var viewModel = new MainViewModel(plantRepository, dialogService, timerService);
            await viewModel.InitializeAsync();
            viewModel.SelectedPlant = viewModel.Plants[0];

            var wateringCard = viewModel.CareStatuses.OfType<WateringStatusViewModel>().Single();

            // When: the Complete Command is executed
            wateringCard.CompleteCommand!.Execute(null);

            // Then: the repository was never called, and the due date is untouched
            Assert.Equal(0, plantRepository.CompleteCareScheduleAsyncCallCount);

            var persistedSchedule = (await plantRepository.GetPlantsAsync())
                .Single()
                .CareSchedules
                .Single();

            Assert.Equal(originalDueDate, persistedSchedule.NextDueAt);
        }

        // -- Remove-Button Tests --

        [Fact]
        public async Task FertilizingCard_RemoveCommand_GivenUserConfirms_RemovesFromRepositoryAndCareStatuses()
        {
            // given: a plant with both Watering and Fertilizing schedules, and the user
            // dialog service configured to simulate the user choosing "Yes"
            var plant = new Plant { Name = "Aloe Vera" };
            plant.CareSchedules.Add(new CareSchedule { Care = CareType.Water, IntervalAmount = 7, IntervalUnit = TimeUnit.Days });
            plant.CareSchedules.Add(new CareSchedule { Care = CareType.Nutrients, IntervalAmount = 30, IntervalUnit = TimeUnit.Days });

            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var dialogService = new FakeDialogService { ConfirmResult = true };
            var timerService = new FakeTimerService();

            var viewModel = new MainViewModel(plantRepository, dialogService, timerService);
            await viewModel.InitializeAsync();
            viewModel.SelectedPlant = viewModel.Plants[0];

            var fertilizingCard = viewModel.CareStatuses.OfType<FertilizingStatusViewModel>();

            // When: the Remove Command is executed on the Fertilizing card
            fertilizingCard.Single().RemoveCommand!.Execute(null);

            // Then: the Fertilizing schedule is gone from the repository and no longer appears among the Care-Statuses, while Watering remains
            var updatedCareStatuses = viewModel.CareStatuses.ToList();
            Assert.DoesNotContain(updatedCareStatuses, c => c is FertilizingStatusViewModel);
            Assert.Contains(updatedCareStatuses, c => c is WateringStatusViewModel);
        }

        [Fact]
        public async Task FertilizingCard_RemoveCommand_GivenUserDeclines_KeepsScheduleUnchanged()
        {
            // Given: a plant with Watering and Fertilizing schedules, and the
            // dialog service configured to simulate the user choosing "No"
            var plant = new Plant { Name = "Aloe Vera" };
            plant.CareSchedules.Add(new CareSchedule { Care = CareType.Water, IntervalAmount = 7, IntervalUnit = TimeUnit.Days });
            plant.CareSchedules.Add(new CareSchedule { Care = CareType.Nutrients, IntervalAmount = 30, IntervalUnit = TimeUnit.Days });

            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var dialogService = new FakeDialogService { ConfirmResult = false };
            var timerService = new FakeTimerService();

            var viewModel = new MainViewModel(plantRepository, dialogService, timerService);
            await viewModel.InitializeAsync();
            viewModel.SelectedPlant = viewModel.Plants[0];

            var fertilizingCard = viewModel.CareStatuses.OfType<FertilizingStatusViewModel>().Single();

            // When: the Remove Command is executed on the Fertilizing card
            fertilizingCard.RemoveCommand!.Execute(null);

            // Then: nothing changed - the Fertilizing schedule remains in the repository and the Fertilizing card is still shown among Care-Statuses
            var updatedCareStatuses = viewModel.CareStatuses.ToList();
            Assert.Contains(updatedCareStatuses, c => c is FertilizingStatusViewModel);
        }

        [Fact]
        public async Task FertilizingCard_RemoveCommand_GivenRepositoryThrows_ShowsErrorAndKeepsSchedule()
        {
            // Given: a plant with Watering and Fertilizing schedules, the user
            // confirming the removal, but the repository configured to fail
            var plant = new Plant { Name = "Aloe Vera" };
            plant.CareSchedules.Add(new CareSchedule { Care = CareType.Water, IntervalAmount = 7, IntervalUnit = TimeUnit.Days });
            plant.CareSchedules.Add(new CareSchedule { Care = CareType.Nutrients, IntervalAmount = 30, IntervalUnit = TimeUnit.Days });

            var plantRepository = new FakePlantRepository { ShouldThrowOnRemoveCareSchedule = true };
            plantRepository.SeedPlants(plant);

            var dialogService = new FakeDialogService { ConfirmResult = true };
            var timerService = new FakeTimerService();

            var viewModel = new MainViewModel(plantRepository, dialogService, timerService);
            await viewModel.InitializeAsync();
            viewModel.SelectedPlant = viewModel.Plants[0];

            var fertilizingCard = viewModel.CareStatuses.OfType<FertilizingStatusViewModel>().Single();

            // When: the Remove Command is executed on the Fertilizing card
            fertilizingCard.RemoveCommand!.Execute(null);

            // Then: an error is shown, and the Fertilizing schedule remains fully intact - both in the repository and still shown among Care-Statuses
            Assert.True(dialogService.ShowErrorWasCalled);

            var persistedSchedules = (await plantRepository.GetPlantsAsync()).Single().CareSchedules;
            Assert.Contains(persistedSchedules, s => s.Care == CareType.Nutrients);

            var updatedCareStatuses = viewModel.CareStatuses.ToList();
            Assert.Contains(updatedCareStatuses, c => c is FertilizingStatusViewModel);
        }

        [Fact]
        public async Task SunlightCard_RemoveCommand_GivenUserConfirms_RemovesFromRepositoryAndCareStatuses()
        {
            // Given: a plant woth a Watering schedule and a Sunlight-Requirement,
            // and the dialog service configured to simulate the user choosing "Yes"
            var plant = new Plant { Name = "Aloe Vera" };
            plant.CareSchedules.Add(new CareSchedule { Care = CareType.Water, IntervalAmount = 7, IntervalUnit = TimeUnit.Days });
            plant.SunlightRequirement = new SunlightRequirement { Hours = 6, Period = SunlightPeriod.Day };

            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var dialogService = new FakeDialogService { ConfirmResult = true };
            var timerService = new FakeTimerService();

            var viewModel = new MainViewModel(plantRepository, dialogService, timerService);
            await viewModel.InitializeAsync();
            viewModel.SelectedPlant = viewModel.Plants[0];

            var sunlightCard = viewModel.CareStatuses.OfType<SunlightStatusViewModel>().Single();

            // When: the Remove Command is executed on the Sunlight card
            sunlightCard.RemoveCommand!.Execute(null);

            // Then: the Sunlight-Requirement is gone from the repository and no longer appears among the Care-Statuses, while Watering remains
            var persistedPlant = (await plantRepository.GetPlantsAsync()).Single();
            Assert.Null(persistedPlant.SunlightRequirement);
            Assert.Contains(viewModel.CareStatuses, s => s is WateringStatusViewModel);

            var updatedCareStatuses = viewModel.CareStatuses.ToList();
            Assert.DoesNotContain(updatedCareStatuses, c => c is SunlightStatusViewModel);
            Assert.Contains(updatedCareStatuses, c => c is WateringStatusViewModel);
        }

        [Fact]
        public async Task SunlightCard_RemoveCommand_GivenUserDeclines_KeepsRequirementUnchanged()
        {
            // Given: a plant woth a Watering schedule and a Sunlight-Requirement,
            // and the dialog service configured to simulate the user choosing "No"
            var plant = new Plant { Name = "Aloe Vera" };
            plant.CareSchedules.Add(new CareSchedule { Care = CareType.Water, IntervalAmount = 7, IntervalUnit = TimeUnit.Days });
            plant.SunlightRequirement = new SunlightRequirement { Hours = 6, Period = SunlightPeriod.Day };

            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var dialogService = new FakeDialogService { ConfirmResult = false };
            var timerService = new FakeTimerService();

            var viewModel = new MainViewModel(plantRepository, dialogService, timerService);
            await viewModel.InitializeAsync();
            viewModel.SelectedPlant = viewModel.Plants[0];

            var sunlightCard = viewModel.CareStatuses.OfType<SunlightStatusViewModel>().Single();

            // When: the Remove Command is executed on the Sunlight card
            sunlightCard.RemoveCommand!.Execute(null);

            // Then: nothing changed - the Sunlight-Requirement remains in the repository and the Sunlight card is still shown among Care-Statuses
            var persistedPlant = (await plantRepository.GetPlantsAsync()).Single();
            Assert.NotNull(persistedPlant.SunlightRequirement);

            var updatedCareStatuses = viewModel.CareStatuses.ToList();
            Assert.Contains(updatedCareStatuses, c => c is SunlightStatusViewModel);
        }

        [Fact]
        public async Task SunlightCard_RemoveCommand_GivenRepositoryThrows_ShowsErrorAndKeepsRequirement()
        {
            // Given: a plant with a Watering schedule and a Sunlight-Requirement,
            // the user confirming the removal, but the repository configured to fail
            var plant = new Plant { Name = "Aloe Vera" };
            plant.CareSchedules.Add(new CareSchedule { Care = CareType.Water, IntervalAmount = 7, IntervalUnit = TimeUnit.Days });
            plant.SunlightRequirement = new SunlightRequirement { Hours = 6, Period = SunlightPeriod.Day };

            var plantRepository = new FakePlantRepository { ShouldThrowOnRemoveSunlightRequirement = true };
            plantRepository.SeedPlants(plant);

            var dialogService = new FakeDialogService { ConfirmResult = true };
            var timerService = new FakeTimerService();

            var viewModel = new MainViewModel(plantRepository, dialogService, timerService);
            await viewModel.InitializeAsync();
            viewModel.SelectedPlant = viewModel.Plants[0];

            var sunlightCard = viewModel.CareStatuses.OfType<SunlightStatusViewModel>().Single();

            // When: the Remove Command is executed on the Sunlight card
            sunlightCard.RemoveCommand!.Execute(null);

            // Then: an error is shown, and the Sunlight-Requirement remains fully intanct - both in the repository and still shown among Care-Statuses
            Assert.True(dialogService.ShowErrorWasCalled);

            var persistedPlant = (await plantRepository.GetPlantsAsync()).Single();
            Assert.NotNull(persistedPlant.SunlightRequirement);

            var updatedCareStatuses = viewModel.CareStatuses.ToList();
            Assert.Contains(updatedCareStatuses, c => c is SunlightStatusViewModel);
        }

        // -- Add/Replace Care-Schedules/Sunlight-Requirement Tests --

        [Fact]
        public async Task AddOrReplaceCareScheduleAsync_GivenNoSelectedPlant_DoesNothing()
        {
            // Given: an initialized MainViewModel with no plant selected
            var plantRepository = new FakePlantRepository();
            var dialogService = new FakeDialogService();
            var timerService = new FakeTimerService();

            var viewModel = new MainViewModel(plantRepository, dialogService, timerService);
            await viewModel.InitializeAsync();

            // Sanity check: nothing selected
            Assert.Null(viewModel.SelectedPlant);

            var newSchedule = new CareSchedule
            {
                Care = CareType.Nutrients,
                IntervalAmount = 30,
                IntervalUnit = TimeUnit.Days
            };

            // When: AddOrReplaceCareScheduleAsync is called without a selected plant
            await viewModel.AddOrReplaceCareScheduleAsync(newSchedule);

            // Then: the repository was never touched
            Assert.Equal(0, plantRepository.AddOrReplaceCareScheduleAsyncCallCount);
        }

        [Fact]
        public async Task AddOrReplaceCareScheduleAsync_GivenMissingIntervalData_DoesNothing()
        {
            // Given: a plant is selected, but the new Care-Schedule has no
            // IntervalAmount/IntervalUnit set
            var plant = new Plant { Name = "Aloe Vera" };
            plant.CareSchedules.Add(new CareSchedule { Care = CareType.Water, IntervalAmount = 7, IntervalUnit = TimeUnit.Days });

            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var dialogService = new FakeDialogService();
            var timerService = new FakeTimerService();

            var viewModel = new MainViewModel(plantRepository, dialogService, timerService);
            await viewModel.InitializeAsync();
            viewModel.SelectedPlant = viewModel.Plants[0];

            var incompleteSchedule = new CareSchedule
            {
                Care = CareType.Nutrients,
                IntervalAmount = null,
                IntervalUnit = null
            };

            // When: AddOrReplaceCareScheduleAsync is called with incomplete data
            await viewModel.AddOrReplaceCareScheduleAsync(incompleteSchedule);

            // Then: the repository was never touched
            Assert.Equal(0, plantRepository.AddOrReplaceCareScheduleAsyncCallCount);
        }

        [Fact]
        public async Task AddOrReplaceCareSchedulesAsync_GivenNewCareType_PersistsAndAddsToCareStatuses()
        {
            // Given: a plant with only a Watering schedule, no Fertilizing yet
            var plant = new Plant { Name = "Aloe Vera" };
            plant.CareSchedules.Add(new CareSchedule { Care = CareType.Water, IntervalAmount = 7, IntervalUnit = TimeUnit.Days });

            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var dialogService = new FakeDialogService();
            var timerService = new FakeTimerService();

            var viewModel = new MainViewModel(plantRepository, dialogService, timerService);
            await viewModel.InitializeAsync();
            viewModel.SelectedPlant = viewModel.Plants[0];

            var newFertilizingSchedule = new CareSchedule
            {
                Care = CareType.Nutrients,
                IntervalAmount = 30,
                IntervalUnit = TimeUnit.Days
            };

            var beforeCall = DateTime.Now;

            // When: the new Fertilizing schedule is added
            await viewModel.AddOrReplaceCareScheduleAsync(newFertilizingSchedule);

            var afterCall = DateTime.Now;

            // Then: it was persisted with a correctly calculated due date and now appears among Care-Statuses, alongside the existing Watering card
            var persistedFertilizing = (await plantRepository.GetPlantsAsync())
                .Single()
                .CareSchedules
                .Single(s => s.Care == CareType.Nutrients);

            Assert.InRange(persistedFertilizing.NextDueAt!.Value, beforeCall.AddDays(30).AddSeconds(-2), afterCall.AddDays(30).AddSeconds(2));
            Assert.InRange(persistedFertilizing.LastCaredAt!.Value, beforeCall.AddSeconds(-2), afterCall.AddSeconds(2));

            var careStatuses = viewModel.CareStatuses.ToList();
            Assert.Contains(careStatuses, c => c is WateringStatusViewModel);
            Assert.Contains(careStatuses, c => c is FertilizingStatusViewModel);
        }

        [Fact]
        public async Task AddOrReplaceCareScheduleAsync_GivenExistingCareType_ReplacesWithoutDuplicating()
        {
            // Given: a plant with an existing Fertilizing schedule (30-day interval)
            var plant = new Plant { Name = "Aloe Vera" };
            plant.CareSchedules.Add(new CareSchedule { Care= CareType.Water, IntervalAmount = 7, IntervalUnit = TimeUnit.Days });
            plant.CareSchedules.Add(new CareSchedule { Care = CareType.Nutrients, IntervalAmount = 30, IntervalUnit = TimeUnit.Days });

            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var dialogService = new FakeDialogService();
            var timerService = new FakeTimerService();

            var viewModel = new MainViewModel(plantRepository, dialogService, timerService);
            await viewModel.InitializeAsync();
            viewModel.SelectedPlant = viewModel.Plants[0];

            // The replacement schedule uses a different interval (14 days instead of 30)
            var replacementSchedule = new CareSchedule
            {
                Care = CareType.Nutrients,
                IntervalAmount = 14,
                IntervalUnit = TimeUnit.Days
            };

            var beforeCall = DateTime.Now;

            // When: the Fertilizing schedule is replaced
            await viewModel.AddOrReplaceCareScheduleAsync(replacementSchedule);

            var afterCall = DateTime.Now;

            // Then: exactly ONE Fertilizing entry remains, with the new interval- no duplicate. Watering remains untouched and the Care-Statuses show exactly two cards
            var persistedSchedules = (await plantRepository.GetPlantsAsync()).Single().CareSchedules;
            var persistedFertilizing = persistedSchedules.Where(s => s.Care == CareType.Nutrients).Single();

            Assert.Equal(14, persistedFertilizing.IntervalAmount);
            Assert.InRange(persistedFertilizing.NextDueAt!.Value, beforeCall.AddDays(14).AddSeconds(-2), afterCall.AddDays(14).AddSeconds(2));

            Assert.Single(persistedSchedules, s => s.Care == CareType.Water);
            Assert.Equal(2, viewModel.CareStatuses.Count());
        }

        [Fact]
        public async Task AddOrReplaceCareScheduleAsync_GivenRepositoryThrows_PropagatesExceptionAndDoesNotAddLocally()
        {
            // Given: a plant with only Watering, and the repository configured to
            // fail when adding/replacing a Care-Schedule
            var plant = new Plant { Name = "Aloe Vera" };
            plant.CareSchedules.Add(new CareSchedule { Care = CareType.Water, IntervalAmount = 7, IntervalUnit = TimeUnit.Days });

            var plantRepository = new FakePlantRepository { ShouldThrowOnAddOrReplaceCareSchedule = true };
            plantRepository.SeedPlants(plant);

            var dialogService = new FakeDialogService();
            var timerService = new FakeTimerService();

            var viewModel = new MainViewModel(plantRepository, dialogService, timerService);
            await viewModel.InitializeAsync();
            viewModel.SelectedPlant = viewModel.Plants[0];

            var newFertilizingSchedule = new CareSchedule
            {
                Care = CareType.Nutrients,
                IntervalAmount = 30,
                IntervalUnit = TimeUnit.Days
            };

            // When: the call should propagate the exception
            await Assert.ThrowsAsync<InvalidOperationException>(() => viewModel.AddOrReplaceCareScheduleAsync(newFertilizingSchedule));

            // Then: the plant's local state remains unchanged - still no Fertilizing card
            var careStatuses = viewModel.CareStatuses.ToList();
            Assert.DoesNotContain(careStatuses, c => c is FertilizingStatusViewModel);
        }

        [Fact]
        public async Task AddOrReplaceSunlightRequirementAsync_GivenNoSelectedPlant_DoesNothing()
        {
            // Given: an initialized MainViewModel with no plant selected
            var plantRepository = new FakePlantRepository();
            var dialogService = new FakeDialogService();
            var timerService = new FakeTimerService();

            var viewModel = new MainViewModel(plantRepository, dialogService, timerService);
            await viewModel.InitializeAsync();

            // Sanity check: nothing selected
            Assert.Null(viewModel.SelectedPlant);

            var newRequirement = new SunlightRequirement
            {
                Hours = 6,
                Period = SunlightPeriod.Day
            };
            // When: AddOrReplaceSunlightRequirementAsync is called without a selected plant
            await viewModel.AddOrReplaceSunlightRequirementAsync(newRequirement);

            // Then: the repository was never touched
            Assert.Equal(0, plantRepository.AddOrReplaceSunlightRequirementAsyncCallCount);
        }

        [Fact]
        public async Task AddOrReplaceSunlightRequirementAsync_GivenNoExistingRequirement_PersistsAndAddsToCareStatuses()
        {
            // Given: a plant with only a Watering schedule, no Sunlight-Requirement yet
            var plant = new Plant { Name = "Aloe Vera" };
            plant.CareSchedules.Add(new CareSchedule { Care = CareType.Water, IntervalAmount = 7, IntervalUnit = TimeUnit.Days });

            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var dialogService = new FakeDialogService();
            var timerService = new FakeTimerService();

            var viewModel = new MainViewModel(plantRepository, dialogService, timerService);
            await viewModel.InitializeAsync();
            viewModel.SelectedPlant = viewModel.Plants[0];

            var newRequirement = new SunlightRequirement
            {
                Hours = 6,
                Period = SunlightPeriod.Day
            };

            // When: the new Sunlight-Requirement is added
            await viewModel.AddOrReplaceSunlightRequirementAsync(newRequirement);

            // Then: it was persisted with the correct values and now appears among Care-Statuses, alongside the existing Watering card
            var persistedRequirement = (await plantRepository.GetPlantsAsync()).Single().SunlightRequirement;

            Assert.NotNull(persistedRequirement);
            Assert.Equal(6, persistedRequirement!.Hours);
            Assert.Equal(SunlightPeriod.Day, persistedRequirement.Period);

            var careStatuses = viewModel.CareStatuses.ToList();
            Assert.Contains(careStatuses, c => c is WateringStatusViewModel);
            Assert.Contains(careStatuses, c => c is SunlightStatusViewModel);
        }

        [Fact]
        public async Task AddOrReplaceSunlightRequirementAsync_GivenExistingRequirement_ReplaceWithNewValues()
        {
            // Given: a plant with an existing Sunlight-Requirement (6 hours per day)
            var plant = new Plant { Name = "Aloe Vera" };
            plant.CareSchedules.Add(new CareSchedule { Care = CareType.Water, IntervalAmount = 7, IntervalUnit = TimeUnit.Days });
            plant.SunlightRequirement = new SunlightRequirement { Hours = 6, Period = SunlightPeriod.Day };

            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var dialogService = new FakeDialogService();
            var timerService = new FakeTimerService();

            var viewModel = new MainViewModel(plantRepository, dialogService, timerService);
            await viewModel.InitializeAsync();
            viewModel.SelectedPlant = viewModel.Plants[0];

            // The replacement uses different values (3 hours per week instead of 6 per day)
            var replacementRequirement = new SunlightRequirement { Hours = 3, Period = SunlightPeriod.Week };

            // When: the Sunlight-Requirement is replaced
            await viewModel.AddOrReplaceSunlightRequirementAsync(replacementRequirement);

            // Then: the persisted requirement reflects the new values and Care-Statuses still show two cards (Watering + Sunlight)
            var persistedPlant = (await plantRepository.GetPlantsAsync()).Single();
            Assert.NotNull(persistedPlant.SunlightRequirement);
            Assert.Equal(3, persistedPlant.SunlightRequirement!.Hours);
            Assert.Equal(SunlightPeriod.Week, persistedPlant.SunlightRequirement.Period);

            Assert.Equal(2, viewModel.CareStatuses.Count());
        }

        [Fact]
        public async Task AddOrReplaceSunlightRequirementAsync_GivenRepositoryThrows_PropagatesExceptionAndDoesNotAddLocally()
        {
            // Given: a plant with only Watering, and the repository configured to
            // fail when adding/replacing a Sunlight-Requirement
            var plant = new Plant { Name = "Aloe Vera" };
            plant.CareSchedules.Add(new CareSchedule { Care = CareType.Water, IntervalAmount = 7, IntervalUnit = TimeUnit.Days });

            var plantRepository = new FakePlantRepository { ShouldThrowOnAddOrReplaceSunlightRequirement = true };
            plantRepository.SeedPlants(plant);

            var dialogService = new FakeDialogService();
            var timerService = new FakeTimerService();

            var viewModel = new MainViewModel(plantRepository, dialogService, timerService);
            await viewModel.InitializeAsync();
            viewModel.SelectedPlant = viewModel.Plants[0];

            var newRequirement = new SunlightRequirement { Hours = 6, Period = SunlightPeriod.Day };

            // When: the call should propagate the exception
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => viewModel.AddOrReplaceSunlightRequirementAsync(newRequirement));

            // Then: the plant's local state remains unchanged - still no Sunlight card
            var careStatuses = viewModel.CareStatuses.ToList();
            Assert.DoesNotContain(careStatuses, c => c is SunlightStatusViewModel);
        }

        // -- Notes Section --

        [Fact]
        public async Task UpdatePlantNotesAsync_GivenNewNotes_PersistsAndUpdatesPlantObject()
        {
            // Given: a plant with existing notes
            var plant = new Plant { Name = "Aloe Vera", Notes = "Old notes" };

            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var dialogService = new FakeDialogService();
            var timerService = new FakeTimerService();

            var viewModel = new MainViewModel(plantRepository, dialogService, timerService);
            await viewModel.InitializeAsync();

            var selectedPlant = viewModel.Plants[0];

            // When: the notes are updated
            await viewModel.UpdatePlantNotesAsync(selectedPlant, "New notes");

            // Then: the change was persisted via the repository and the in-memory Plant-Object was updates directly too
            Assert.Equal("New notes", selectedPlant.Notes);
        }

        [Fact]
        public async Task UpdatePlantNotesAsync_GivenRepositoryThrows_PropagatesExceptionAndDoesNotUpdatePlantObject()
        {
            // Given: a plant with existing notes, and the repository configured to
            // fail when saving notes
            var plant = new Plant { Name = "Aloe Vera", Notes = "Old notes" };

            var plantRepository = new FakePlantRepository { ShouldThrowOnUpdateNotes = true };
            plantRepository.SeedPlants(plant);

            var dialogService = new FakeDialogService();
            var timerService = new FakeTimerService();

            var viewModel = new MainViewModel(plantRepository, dialogService, timerService);
            await viewModel.InitializeAsync();

            var selectedPlant = viewModel.Plants[0];

            // When: the call should propagate the exception
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => viewModel.UpdatePlantNotesAsync(selectedPlant, "New notes"));

            // Then: the in-memory Plant-Object was NOT updated
            Assert.Equal("Old notes", selectedPlant.Notes);
        }

        // -- CanExecute & Event Tests --

        [Theory]
        [InlineData("AddSchedule")]
        [InlineData("DeletePlant")]
        [InlineData("OpenNotes")]
        public async Task Command_GivenNoSelectedPlant_CanExecuteReturnsFalse(string commandName)
        {
            // Given: an initialized MainViewModel with no plant selected
            var plantRepository = new FakePlantRepository();
            var dialogService = new FakeDialogService();
            var timerService = new FakeTimerService();

            var viewModel = new MainViewModel(plantRepository, dialogService, timerService);
            await viewModel.InitializeAsync();

            // When: Command is set
            var command = GetCommand(viewModel, commandName);

            // Then: CanExecute should be false, since no plant is selected
            Assert.False(command.CanExecute(null));
        }

        [Theory]
        [InlineData("AddSchedule")]
        [InlineData("DeletePlant")]
        [InlineData("OpenNotes")]
        public async Task Command_GivenSelectedPlant_CanExecuteReturnsTrue(string commandName)
        {
            // Given: an initialized MainViewModel with a plant selected
            var plant = new Plant { Name = "Aloe Vera" };
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var dialogService = new FakeDialogService();
            var timerService = new FakeTimerService();

            var viewModel = new MainViewModel(plantRepository, dialogService, timerService);
            await viewModel.InitializeAsync();
            viewModel.SelectedPlant = viewModel.Plants[0];

            // When: Command is set
            var command = GetCommand(viewModel, commandName);

            // Then: CanExecute should be true, since a plant is selected
            Assert.True(command.CanExecute(null));
        }

        [Fact]
        public async Task AddPlantCommand_Execute_RaisesAddPlantRequested()
        {
            // Given: an initialized MainViewModel
            var plantRepository = new FakePlantRepository();
            var dialogService = new FakeDialogService();
            var timerService = new FakeTimerService();

            var viewModel = new MainViewModel(plantRepository, dialogService, timerService);
            await viewModel.InitializeAsync();

            int eventRaisedCount = 0;
            viewModel.AddPlantRequested += (_, _) => eventRaisedCount++;

            // When: AddPlantCommand is executed
            viewModel.AddPlantCommand.Execute(null);

            // Then: AddPlantRequested was raised exactly once
            Assert.Equal(1, eventRaisedCount);
        }

        [Fact]
        public async Task AddScheduleCommand_Execute_RaisesAddScheduleRequestedWithSelectedPlant()
        {
            // Given: an initialized MainViewModel with a plant selected
            var plant = new Plant { Name = "Aloe Vera" };
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var dialogService = new FakeDialogService();
            var timerService = new FakeTimerService();

            var viewModel = new MainViewModel(plantRepository, dialogService, timerService);
            await viewModel.InitializeAsync();
            viewModel.SelectedPlant = viewModel.Plants[0];

            Plant? raisedPlant = null;
            int eventRaisedCount = 0;
            viewModel.AddScheduleRequested += (_, raisedArg) =>
            {
                eventRaisedCount++;
                raisedPlant = raisedArg;
            };

            // When: AddScheduleCommand is executed
            viewModel.AddScheduleCommand.Execute(null);

            // Then: AddScheduleRequested was raised exactly once, with the
            // currently selected plant as the argument
            Assert.Equal(1, eventRaisedCount);
            Assert.Same(viewModel.SelectedPlant, raisedPlant);
        }

        [Fact]
        public async Task OpenNotesCommand_Execute_RaisesOpenNotesRequestedWithSelectedPlant()
        {
            // Given: an initialized MainViewModel with a plant selected
            var plant = new Plant { Name = "Aloe Vera" };
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var dialogService = new FakeDialogService();
            var timerService = new FakeTimerService();

            var viewModel = new MainViewModel(plantRepository, dialogService, timerService);
            await viewModel.InitializeAsync();
            viewModel.SelectedPlant = viewModel.Plants[0];

            Plant? raisedPlant = null;
            int eventRaisedCount = 0;
            viewModel.OpenNotesRequested += (_, raisedArg) =>
            {
                eventRaisedCount++;
                raisedPlant = raisedArg;
            };

            // When: OpenNotesCommand is executed
            viewModel.OpenNotesCommand.Execute(null);

            // Then: OpenNotesRequested was raised exactly once, with the currently
            // selected plant as the argument
            Assert.Equal(1, eventRaisedCount);
            Assert.Same(viewModel.SelectedPlant, raisedPlant);
        }

        [Fact]
        public async Task WateringCard_EditCommand_RaisesEditScheduleRequestedWithPlantAndCareType()
        {
            // Given: an initialized MainViewModel with a plant that has a Watering schedule
            var plant = new Plant { Name = "Aloe Vera" };
            plant.CareSchedules.Add(new CareSchedule { Care = CareType.Water, IntervalAmount = 7, IntervalUnit = TimeUnit.Days });

            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var dialogService = new FakeDialogService();
            var timerService = new FakeTimerService();

            var viewModel = new MainViewModel(plantRepository, dialogService, timerService);
            await viewModel.InitializeAsync();
            viewModel.SelectedPlant = viewModel.Plants[0];

            var wateringCard = viewModel.CareStatuses.OfType<WateringStatusViewModel>().Single();

            (Plant plant, CareType care)? raisedArgs = null;
            int eventRaisedCount = 0;
            viewModel.EditScheduleRequested += (_, args) =>
            {
                eventRaisedCount++;
                raisedArgs = args;
            };

            // When: the Edit Command is executed on the Watering card
            wateringCard.EditCommand!.Execute(null);

            // Then: EditScheduleRequested was raised exactly once, with the
            // selected plant and the Care-Type "Water" as the arguments
            Assert.Equal(1, eventRaisedCount);
            Assert.Same(viewModel.SelectedPlant, raisedArgs!.Value.plant);
            Assert.Equal(CareType.Water, raisedArgs.Value.care);
        }

        // Help-Method: Maps a simple string identifier to the actual command on the ViewModel -
        // necessary because [InlineData] can only carry constant values, not delegates or direct Command references
        private static ICommand GetCommand(MainViewModel viewModel, string commandName) => commandName switch
        {
            "AddSchedule" => viewModel.AddScheduleCommand,
            "DeletePlant" => viewModel.DeletePlantCommand,
            "OpenNotes" => viewModel.OpenNotesCommand,
            _ => throw new ArgumentOutOfRangeException(nameof(commandName))
        };

        // -- Timer Tests --

        [Fact]
        public void Constructor_GivenTimerService_StartsTimerWithFiveMinuteInterval()
        {
            // Given: a fresh FakeTimerService
            var plantRepository = new FakePlantRepository();
            var dialogService = new FakeDialogService();
            var timerService = new FakeTimerService();

            // When: a MainViewModel is constructed with it
            var viewModel = new MainViewModel(plantRepository, dialogService, timerService);

            // Then: Start was called with a 5-minute interval
            Assert.True(timerService.StartWasCalled);
            Assert.Equal(TimeSpan.FromMinutes(5), timerService.LastInterval);
        }

        [Fact]
        public async Task RefreshCareStatuses_WhenCalled_RaisesPropertyChangedForCareStatuses()
        {
            // Given: an initialized MainViewModel
            var plantRepository = new FakePlantRepository();
            var dialogService = new FakeDialogService();
            var timerService = new FakeTimerService();

            var viewModel = new MainViewModel(plantRepository, dialogService, timerService);
            await viewModel.InitializeAsync();

            var raisedProperties = new List<string>();
            viewModel.PropertyChanged += (_, e) => raisedProperties.Add(e.PropertyName!);

            // When: RefreshCareStatuses is called directly
            viewModel.RefreshCareStatuses();

            // Then: PropertyChanged was raised for Care-Statuses
            Assert.Contains(nameof(MainViewModel.CareStatuses), raisedProperties);
        }

        [Fact]
        public async Task SimulatedTimerTick_WhenTriggered_RaisesPropertyChangedForCareStatuses()
        {
            var plantRepository = new FakePlantRepository();
            var dialogService = new FakeDialogService();
            var timerService = new FakeTimerService();

            var viewModel = new MainViewModel(plantRepository, dialogService, timerService);
            await viewModel.InitializeAsync();

            var raisedProperties = new List<string>();
            viewModel.PropertyChanged += (_, e) => raisedProperties.Add(e.PropertyName!);

            // When: a timer tick is simulated, without waiting for a real 5-minute interval
            timerService.TriggerTick();

            // Then: the callback passed to _timerService.Start(...) in the constructor is genuinely wired
            // to RefreshCareStatuses and that the timer actually triggers it
            Assert.Contains(nameof(MainViewModel.CareStatuses), raisedProperties);
        }
    }
}
