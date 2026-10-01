using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.Tests.Fakes;
using GreenKeeper.ViewModels;
using static GreenKeeper.Tests.TestPlants;
using GreenKeeper.ViewModels.CareStatuses.Active;
using GreenKeeper.ViewModels.CareStatuses.Passive;
using GreenKeeper.ViewModels.Themes;
using System.Windows.Input;

namespace GreenKeeper.Tests.ViewModels
{
    public class MainViewModelTests
    {
        // The clock every ViewModel under test runs on, so due dates are exact instead of "now".
        private static readonly DateTime Now = new(2025, 5, 1, 10, 30, 0);

        // -- Basic Tests --

        [Fact]
        public async Task InitializeAsync_GivenRepositoryWithOnePlant_PopulatesPlants()
        {
            // Given: a repository seeded with one existing plant
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(AloeVera());

            var viewModel = CreateViewModel(plantRepository);

            // When: InitializeAsync is called
            await viewModel.InitializeAsync();

            // Then: Plants should contain exactly the seeded plant
            Assert.Single(viewModel.Plants);
            Assert.Equal("Aloe Vera", viewModel.Plants[0].Name);
        }

        // -- Add Plant Tests --

        [Fact]
        public async Task AddPlantAsync_GivenExistingPlants_AppendsWithoutRemovingExisting()
        {
            // Given: a repository already containing one plant
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(AloeVera());

            var viewModel = await CreateInitializedViewModelAsync(plantRepository);

            // When: a second plant is added
            await viewModel.AddPlantAsync(new Plant { Name = "Basil" });

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
            var viewModel = CreateViewModel(plantRepository);

            await viewModel.InitializeAsync();

            // When: a plant is added
            var exception = await Record.ExceptionAsync(() => viewModel.AddPlantAsync(new Plant { Name = "Basil" }));

            // Then: the exception is propagated and the plant does NOT appear in the UI-bound collection
            Assert.IsType<InvalidOperationException>(exception);
            Assert.Empty(viewModel.Plants);
        }

        [Fact]
        public async Task AddPlantAsync_GivenPlantWithSchedules_StartsTheirIntervalsNow()
        {
            // Given: a plant from the wizard, watered every 7 days and fertilized every 30 days, without due dates yet
            var plantRepository = new FakePlantRepository();
            var viewModel = await CreateInitializedViewModelAsync(plantRepository);
            var plant = AloeVera(WateringEveryDays(7), FertilizingEveryDays(30));

            // When: the plant is added
            await viewModel.AddPlantAsync(plant);

            // Then: every schedule counts as cared for now and is due one interval later
            var persisted = (await plantRepository.GetPlantsAsync()).Single();
            var watering = persisted.CareSchedules.Single(s => s.Care == CareType.Watering);
            var fertilizing = persisted.CareSchedules.Single(s => s.Care == CareType.Fertilizing);
            Assert.Equal(Now.AddDays(7), watering.NextDueAt);
            Assert.Equal(Now.AddDays(30), fertilizing.NextDueAt);
            Assert.Equal(Now, watering.LastCaredAt);
            Assert.Equal(Now, fertilizing.LastCaredAt);
        }

        // -- Delete Plant --

        [Fact]
        public async Task DeletePlantCommand_GivenUserConfirms_RemovesPlantFromRepositoryAndCollection()
        {
            // Given: a repository with one plant, currently selected, and the
            // dialog service configured to simulate the user choosing "Yes"
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(AloeVera());

            var dialogService = new FakeDialogService { ConfirmResult = true };

            var viewModel = await CreateInitializedViewModelAsync(plantRepository, dialogService);
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
            plantRepository.SeedPlants(AloeVera());

            var dialogService = new FakeDialogService { ConfirmResult = false };

            var viewModel = await CreateInitializedViewModelAsync(plantRepository, dialogService);
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
            plantRepository.SeedPlants(AloeVera());

            var dialogService = new FakeDialogService { ConfirmResult = true };

            var viewModel = await CreateInitializedViewModelAsync(plantRepository, dialogService);
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

        // -- Plant Selection Tests --

        [Fact]
        public async Task SelectedPlant_GivenPlantIsSelected_UpdatesIsPlantSelectedAndRaisesPropertyChanged()
        {
            // Given: an initialized MainViewModel with one plant, nothing selected yet
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(AloeVera());

            var viewModel = await CreateInitializedViewModelAsync(plantRepository);

            // Sanity check on the initial state, before the actual "When" happens.
            Assert.False(viewModel.IsPlantSelected);

            var raisedProperties = new List<string>();
            viewModel.PropertyChanged += (_, e) => raisedProperties.Add(e.PropertyName!);

            // When: a plant is selected
            viewModel.SelectedPlant = viewModel.Plants[0];

            // Then: IsPlantSelected reflects the new state, and PropertyChanged was
            // raised for all three properties that depend on the selection.
            // CareStatuses matters just as much as the other two: without its
            // notification the status cards would keep showing the previous plant
            Assert.True(viewModel.IsPlantSelected);
            Assert.Contains(nameof(MainViewModel.SelectedPlant), raisedProperties);
            Assert.Contains(nameof(MainViewModel.IsPlantSelected), raisedProperties);
            Assert.Contains(nameof(MainViewModel.CareStatuses), raisedProperties);
        }

        // -- Search Tests --

        [Fact]
        public async Task SearchText_GivenPlantIsSelected_ResetsSelectedPlantToNull()
        {
            // Given: an initialized MainViewModel with a plant currently selected
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(AloeVera());

            var viewModel = await CreateInitializedViewModelAsync(plantRepository);
            viewModel.SelectedPlant = viewModel.Plants[0];

            // Sanity check before the actual "When".
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
            plantRepository.SeedPlants(AloeVera());

            var viewModel = await CreateInitializedViewModelAsync(plantRepository);
            viewModel.SearchText = "al";
            viewModel.SelectedPlant = viewModel.Plants[0];

            // When: SearchText is set to the exact same value again
            viewModel.SearchText = "al";

            // Then: nothing actually changed, so the selection should be preserved
            // (the setter's early-return guard for unchanged values should prevent
            // the deselection logic from running again)
            Assert.NotNull(viewModel.SelectedPlant);
        }

        // -- CareStatuses Tests --

        [Fact]
        public async Task CareStatuses_GivenPlantWithOnlyWatering_ReturnsOnlyWateringCard()
        {
            // Given: a plant with only a care schedule for Watering, no Fertilizing, no Sunlight
            var plant = PlantNamed("Cactus", WateringEveryDays(7));

            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var viewModel = await CreateInitializedViewModelAsync(plantRepository);
            viewModel.SelectedPlant = viewModel.Plants[0];

            // When: CareStatuses is read
            var careStatuses = viewModel.CareStatuses.ToList();

            // Then: exactly one card for Watering
            Assert.Single(careStatuses);
            Assert.IsType<WateringStatusViewModel>(careStatuses[0]);
        }

        [Fact]
        public async Task CareStatuses_GivenPlantWithAllCareTypes_ReturnsAllThreeStatusCardsInOrder()
        {
            // Given: a plant with Watering, Fertilizing and SunlightRequirement all set
            var plant = AloeVera(WateringEveryDays(7), FertilizingEveryDays(30));
            plant.SunlightRequirement = DailySunlight(6);

            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var viewModel = await CreateInitializedViewModelAsync(plantRepository);
            viewModel.SelectedPlant = viewModel.Plants[0];

            // When: CareStatuses is read
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
            var plant = PlantNamed("Snake Plant", WateringEveryDays(14));
            plant.SunlightRequirement = DailySunlight(4);

            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var viewModel = await CreateInitializedViewModelAsync(plantRepository);
            viewModel.SelectedPlant = viewModel.Plants[0];

            // When: CareStatuses is read
            var careStatuses = viewModel.CareStatuses.ToList();

            // Then: exactly Watering and Sunlight, no Fertilizing between them
            Assert.Equal(2, careStatuses.Count);
            Assert.IsType<WateringStatusViewModel>(careStatuses[0]);
            Assert.IsType<SunlightStatusViewModel>(careStatuses[1]);
        }

        // -- Complete Button Tests --

        [Fact]
        public async Task WateringCard_CompleteCommand_GivenValidSchedule_RecalculatesAndPersistsDueDate()
        {
            // Given: a plant with an overdue Watering care schedule
            var plant = AloeVera();
            plant.CareSchedules.Add(new CareSchedule
            {
                Care = CareType.Watering,
                IntervalAmount = 7,
                IntervalUnit = TimeUnit.Days,
                NextDueAt = Now.AddDays(-1)
            });

            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var viewModel = await CreateInitializedViewModelAsync(plantRepository);
            viewModel.SelectedPlant = viewModel.Plants[0];

            var wateringCard = viewModel.CareStatuses.OfType<WateringStatusViewModel>().Single();

            // When: CompleteCommand is executed
            wateringCard.CompleteCommand!.Execute(null);

            // Then: the persisted schedule reflects a next due date (NextDueAt) exactly 7 days from now,
            // and the last date of care (LastCaredAt) now - checked via the repository, not
            // just the in-memory ViewModel state, to confirm actual persistence
            var persistedSchedule = (await plantRepository.GetPlantsAsync())
                .Single()
                .CareSchedules
                .Single(s => s.Care == CareType.Watering);

            Assert.Equal(Now.AddDays(7), persistedSchedule.NextDueAt);
            Assert.Equal(Now, persistedSchedule.LastCaredAt);
        }

        [Fact]
        public async Task FertilizingCard_CompleteCommand_GivenValidSchedule_RecalculatesAndPersistsDueDateWithoutAffectingWatering()
        {
            // Given: a plant with both a Watering schedule (untouched reference point) and an overdue Fertilizing schedule
            var plant = AloeVera();
            var originalWateringDueDate = Now.AddDays(3);

            plant.CareSchedules.Add(new CareSchedule
            {
                Care = CareType.Watering,
                IntervalAmount = 7,
                IntervalUnit = TimeUnit.Days,
                NextDueAt = originalWateringDueDate
            });
            plant.CareSchedules.Add(new CareSchedule
            {
                Care = CareType.Fertilizing,
                IntervalAmount = 30,
                IntervalUnit = TimeUnit.Days,
                NextDueAt = Now.AddDays(-1)
            });

            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var viewModel = await CreateInitializedViewModelAsync(plantRepository);
            viewModel.SelectedPlant = viewModel.Plants[0];

            var fertilizingCard = viewModel.CareStatuses.OfType<FertilizingStatusViewModel>().Single();

            // When: CompleteCommand is executed on the Fertilizing card
            fertilizingCard.CompleteCommand!.Execute(null);

            // Then: only the Fertilizing schedule was recalculated and persisted, while the due date of Watering remains completely untouched
            var persistedSchedules = (await plantRepository.GetPlantsAsync()).Single().CareSchedules;
            var persistedFertilizing = persistedSchedules.Single(s => s.Care == CareType.Fertilizing);
            var persistedWatering = persistedSchedules.Single(s => s.Care == CareType.Watering);

            Assert.Equal(Now.AddDays(30), persistedFertilizing.NextDueAt);
            Assert.Equal(Now, persistedFertilizing.LastCaredAt);

            Assert.Equal(originalWateringDueDate, persistedWatering.NextDueAt);
            Assert.Null(persistedWatering.LastCaredAt);
        }

        [Fact]
        public async Task WateringCard_CompleteCommand_GivenMissingIntervalData_DoesNothing()
        {
            // Given: a plant with a Watering schedule that has NO IntervalAmount/IntervalUnit set, but the next due date (NextDueAt) is still present
            var originalDueDate = Now.AddDays(-1);
            var plant = AloeVera();
            plant.CareSchedules.Add(new CareSchedule
            {
                Care = CareType.Watering,
                IntervalAmount = null,
                IntervalUnit = null,
                NextDueAt = originalDueDate
            });

            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var viewModel = await CreateInitializedViewModelAsync(plantRepository);
            viewModel.SelectedPlant = viewModel.Plants[0];

            var wateringCard = viewModel.CareStatuses.OfType<WateringStatusViewModel>().Single();

            // When: CompleteCommand is executed
            wateringCard.CompleteCommand!.Execute(null);

            // Then: the repository was never called, and the due date is untouched
            Assert.Equal(0, plantRepository.CompleteCareScheduleAsyncCallCount);

            var persistedSchedule = (await plantRepository.GetPlantsAsync())
                .Single()
                .CareSchedules
                .Single();

            Assert.Equal(originalDueDate, persistedSchedule.NextDueAt);
        }

        // -- Remove Button Tests --

        [Fact]
        public async Task FertilizingCard_RemoveCommand_GivenUserConfirms_RemovesFromRepositoryAndCareStatuses()
        {
            // Given: a plant with both Watering and Fertilizing schedules, and the
            // dialog service configured to simulate the user choosing "Yes"
            var plant = AloeVera(WateringEveryDays(7), FertilizingEveryDays(30));

            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var dialogService = new FakeDialogService { ConfirmResult = true };

            var viewModel = await CreateInitializedViewModelAsync(plantRepository, dialogService);
            viewModel.SelectedPlant = viewModel.Plants[0];

            var fertilizingCard = viewModel.CareStatuses.OfType<FertilizingStatusViewModel>();

            // When: RemoveCommand is executed on the Fertilizing card
            fertilizingCard.Single().RemoveCommand!.Execute(null);

            // Then: Fertilizing is gone from the repository and from the cards, while
            // Watering stays. The call count is what proves the repository was asked
            // at all - the ViewModel drops the schedule from the same Plant object anyway
            Assert.Equal(1, plantRepository.RemoveCareScheduleAsyncCallCount);

            var persistedSchedules = (await plantRepository.GetPlantsAsync()).Single().CareSchedules;
            Assert.DoesNotContain(persistedSchedules, s => s.Care == CareType.Fertilizing);
            Assert.Contains(persistedSchedules, s => s.Care == CareType.Watering);

            var updatedCareStatuses = viewModel.CareStatuses.ToList();
            Assert.DoesNotContain(updatedCareStatuses, c => c is FertilizingStatusViewModel);
            Assert.Contains(updatedCareStatuses, c => c is WateringStatusViewModel);
        }

        [Fact]
        public async Task FertilizingCard_RemoveCommand_GivenUserDeclines_KeepsScheduleUnchanged()
        {
            // Given: a plant with Watering and Fertilizing schedules, and the
            // dialog service configured to simulate the user choosing "No"
            var plant = AloeVera(WateringEveryDays(7), FertilizingEveryDays(30));

            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var dialogService = new FakeDialogService { ConfirmResult = false };

            var viewModel = await CreateInitializedViewModelAsync(plantRepository, dialogService);
            viewModel.SelectedPlant = viewModel.Plants[0];

            var fertilizingCard = viewModel.CareStatuses.OfType<FertilizingStatusViewModel>().Single();

            // When: RemoveCommand is executed on the Fertilizing card
            fertilizingCard.RemoveCommand!.Execute(null);

            // Then: nothing changed - the Fertilizing schedule remains in the repository and the Fertilizing card is still shown among CareStatuses
            var updatedCareStatuses = viewModel.CareStatuses.ToList();
            Assert.Contains(updatedCareStatuses, c => c is FertilizingStatusViewModel);
        }

        [Fact]
        public async Task FertilizingCard_RemoveCommand_GivenRepositoryThrows_ShowsErrorAndKeepsSchedule()
        {
            // Given: a plant with Watering and Fertilizing schedules, the user
            // confirming the removal, but the repository configured to fail
            var plant = AloeVera(WateringEveryDays(7), FertilizingEveryDays(30));

            var plantRepository = new FakePlantRepository { ShouldThrowOnRemoveCareSchedule = true };
            plantRepository.SeedPlants(plant);

            var dialogService = new FakeDialogService { ConfirmResult = true };

            var viewModel = await CreateInitializedViewModelAsync(plantRepository, dialogService);
            viewModel.SelectedPlant = viewModel.Plants[0];

            var fertilizingCard = viewModel.CareStatuses.OfType<FertilizingStatusViewModel>().Single();

            // When: RemoveCommand is executed on the Fertilizing card
            fertilizingCard.RemoveCommand!.Execute(null);

            // Then: an error is shown, and the Fertilizing schedule remains fully intact - both in the repository and still shown among CareStatuses
            Assert.True(dialogService.ShowErrorWasCalled);

            var persistedSchedules = (await plantRepository.GetPlantsAsync()).Single().CareSchedules;
            Assert.Contains(persistedSchedules, s => s.Care == CareType.Fertilizing);

            var updatedCareStatuses = viewModel.CareStatuses.ToList();
            Assert.Contains(updatedCareStatuses, c => c is FertilizingStatusViewModel);
        }

        [Fact]
        public async Task SunlightCard_RemoveCommand_GivenUserConfirms_RemovesFromRepositoryAndCareStatuses()
        {
            // Given: a plant with a Watering schedule and a sunlight requirement,
            // and the dialog service configured to simulate the user choosing "Yes"
            var plant = AloeVera(WateringEveryDays(7));
            plant.SunlightRequirement = DailySunlight(6);

            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var dialogService = new FakeDialogService { ConfirmResult = true };

            var viewModel = await CreateInitializedViewModelAsync(plantRepository, dialogService);
            viewModel.SelectedPlant = viewModel.Plants[0];

            var sunlightCard = viewModel.CareStatuses.OfType<SunlightStatusViewModel>().Single();

            // When: RemoveCommand is executed on the Sunlight card
            sunlightCard.RemoveCommand!.Execute(null);

            // Then: the sunlight requirement is gone from the repository and from the
            // cards, while Watering stays. The call count is what proves the repository
            // was asked - the ViewModel clears it on the same Plant object anyway
            Assert.Equal(1, plantRepository.RemoveSunlightRequirementAsyncCallCount);

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
            // Given: a plant with a Watering schedule and a sunlight requirement,
            // and the dialog service configured to simulate the user choosing "No"
            var plant = AloeVera(WateringEveryDays(7));
            plant.SunlightRequirement = DailySunlight(6);

            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var dialogService = new FakeDialogService { ConfirmResult = false };

            var viewModel = await CreateInitializedViewModelAsync(plantRepository, dialogService);
            viewModel.SelectedPlant = viewModel.Plants[0];

            var sunlightCard = viewModel.CareStatuses.OfType<SunlightStatusViewModel>().Single();

            // When: RemoveCommand is executed on the Sunlight card
            sunlightCard.RemoveCommand!.Execute(null);

            // Then: nothing changed - the sunlight requirement remains in the repository and the Sunlight card is still shown among CareStatuses
            var persistedPlant = (await plantRepository.GetPlantsAsync()).Single();
            Assert.NotNull(persistedPlant.SunlightRequirement);

            var updatedCareStatuses = viewModel.CareStatuses.ToList();
            Assert.Contains(updatedCareStatuses, c => c is SunlightStatusViewModel);
        }

        [Fact]
        public async Task SunlightCard_RemoveCommand_GivenRepositoryThrows_ShowsErrorAndKeepsRequirement()
        {
            // Given: a plant with a Watering schedule and a sunlight requirement,
            // the user confirming the removal, but the repository configured to fail
            var plant = AloeVera(WateringEveryDays(7));
            plant.SunlightRequirement = DailySunlight(6);

            var plantRepository = new FakePlantRepository { ShouldThrowOnRemoveSunlightRequirement = true };
            plantRepository.SeedPlants(plant);

            var dialogService = new FakeDialogService { ConfirmResult = true };

            var viewModel = await CreateInitializedViewModelAsync(plantRepository, dialogService);
            viewModel.SelectedPlant = viewModel.Plants[0];

            var sunlightCard = viewModel.CareStatuses.OfType<SunlightStatusViewModel>().Single();

            // When: RemoveCommand is executed on the Sunlight card
            sunlightCard.RemoveCommand!.Execute(null);

            // Then: an error is shown, and the sunlight requirement remains fully intact - both in the repository and still shown among CareStatuses
            Assert.True(dialogService.ShowErrorWasCalled);

            var persistedPlant = (await plantRepository.GetPlantsAsync()).Single();
            Assert.NotNull(persistedPlant.SunlightRequirement);

            var updatedCareStatuses = viewModel.CareStatuses.ToList();
            Assert.Contains(updatedCareStatuses, c => c is SunlightStatusViewModel);
        }

        // -- Add/Replace Care Schedules/Sunlight Requirement Tests --

        [Fact]
        public async Task AddOrReplaceCareScheduleAsync_GivenNoSelectedPlant_DoesNothing()
        {
            // Given: an initialized MainViewModel with no plant selected
            var plantRepository = new FakePlantRepository();
            var viewModel = await CreateInitializedViewModelAsync(plantRepository);

            // Sanity check: nothing selected.
            Assert.Null(viewModel.SelectedPlant);

            var newSchedule = new CareSchedule
            {
                Care = CareType.Fertilizing,
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
            // Given: a plant is selected, but the new care schedule has no
            // IntervalAmount/IntervalUnit set
            var plant = AloeVera(WateringEveryDays(7));

            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var viewModel = await CreateInitializedViewModelAsync(plantRepository);
            viewModel.SelectedPlant = viewModel.Plants[0];

            var incompleteSchedule = new CareSchedule
            {
                Care = CareType.Fertilizing,
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
            var plant = AloeVera(WateringEveryDays(7));

            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var viewModel = await CreateInitializedViewModelAsync(plantRepository);
            viewModel.SelectedPlant = viewModel.Plants[0];

            var newFertilizingSchedule = new CareSchedule
            {
                Care = CareType.Fertilizing,
                IntervalAmount = 30,
                IntervalUnit = TimeUnit.Days
            };

            // When: the new Fertilizing schedule is added
            await viewModel.AddOrReplaceCareScheduleAsync(newFertilizingSchedule);

            // Then: it was persisted with a correctly calculated due date and now appears among CareStatuses, alongside the existing Watering card
            var persistedFertilizing = (await plantRepository.GetPlantsAsync())
                .Single()
                .CareSchedules
                .Single(s => s.Care == CareType.Fertilizing);

            Assert.Equal(Now.AddDays(30), persistedFertilizing.NextDueAt);
            Assert.Equal(Now, persistedFertilizing.LastCaredAt);

            var careStatuses = viewModel.CareStatuses.ToList();
            Assert.Contains(careStatuses, c => c is WateringStatusViewModel);
            Assert.Contains(careStatuses, c => c is FertilizingStatusViewModel);
        }

        [Fact]
        public async Task AddOrReplaceCareScheduleAsync_GivenExistingCareType_ReplacesWithoutDuplicating()
        {
            // Given: a plant with an existing Fertilizing schedule (30-day interval)
            var plant = AloeVera(WateringEveryDays(7), FertilizingEveryDays(30));

            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var viewModel = await CreateInitializedViewModelAsync(plantRepository);
            viewModel.SelectedPlant = viewModel.Plants[0];

            // The replacement schedule uses a different interval (14 days instead of 30).
            var replacementSchedule = new CareSchedule
            {
                Care = CareType.Fertilizing,
                IntervalAmount = 14,
                IntervalUnit = TimeUnit.Days
            };

            // When: the Fertilizing schedule is replaced
            await viewModel.AddOrReplaceCareScheduleAsync(replacementSchedule);

            // Then: exactly ONE Fertilizing entry remains, with the new interval - no duplicate. Watering remains untouched and CareStatuses shows exactly two cards
            var persistedSchedules = (await plantRepository.GetPlantsAsync()).Single().CareSchedules;
            var persistedFertilizing = persistedSchedules.Where(s => s.Care == CareType.Fertilizing).Single();

            Assert.Equal(14, persistedFertilizing.IntervalAmount);
            Assert.Equal(Now.AddDays(14), persistedFertilizing.NextDueAt);

            Assert.Single(persistedSchedules, s => s.Care == CareType.Watering);
            Assert.Equal(2, viewModel.CareStatuses.Count());
        }

        [Fact]
        public async Task AddOrReplaceCareScheduleAsync_GivenRepositoryThrows_PropagatesExceptionAndDoesNotAddLocally()
        {
            // Given: a plant with only Watering, and the repository configured to
            // fail when adding/replacing a care schedule
            var plant = AloeVera(WateringEveryDays(7));

            var plantRepository = new FakePlantRepository { ShouldThrowOnAddOrReplaceCareSchedule = true };
            plantRepository.SeedPlants(plant);

            var viewModel = await CreateInitializedViewModelAsync(plantRepository);
            viewModel.SelectedPlant = viewModel.Plants[0];

            var newFertilizingSchedule = new CareSchedule
            {
                Care = CareType.Fertilizing,
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
            var viewModel = await CreateInitializedViewModelAsync(plantRepository);

            // Sanity check: nothing selected.
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
            // Given: a plant with only a Watering schedule, no sunlight requirement yet
            var plant = AloeVera(WateringEveryDays(7));

            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var viewModel = await CreateInitializedViewModelAsync(plantRepository);
            viewModel.SelectedPlant = viewModel.Plants[0];

            var newRequirement = new SunlightRequirement
            {
                Hours = 6,
                Period = SunlightPeriod.Day
            };

            // When: the new sunlight requirement is added
            await viewModel.AddOrReplaceSunlightRequirementAsync(newRequirement);

            // Then: it was persisted exactly once with the correct values and now appears among CareStatuses, alongside the existing Watering card
            Assert.Equal(1, plantRepository.AddOrReplaceSunlightRequirementAsyncCallCount);

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
            // Given: a plant with an existing sunlight requirement (6 hours per day)
            var plant = AloeVera(WateringEveryDays(7));
            plant.SunlightRequirement = DailySunlight(6);

            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var viewModel = await CreateInitializedViewModelAsync(plantRepository);
            viewModel.SelectedPlant = viewModel.Plants[0];

            // The replacement uses different values (3 hours per week instead of 6 per day).
            var replacementRequirement = new SunlightRequirement { Hours = 3, Period = SunlightPeriod.Week };

            // When: the sunlight requirement is replaced
            await viewModel.AddOrReplaceSunlightRequirementAsync(replacementRequirement);

            // Then: the persisted requirement reflects the new values and CareStatuses still shows two cards (Watering + Sunlight)
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
            // fail when adding/replacing a sunlight requirement
            var plant = AloeVera(WateringEveryDays(7));

            var plantRepository = new FakePlantRepository { ShouldThrowOnAddOrReplaceSunlightRequirement = true };
            plantRepository.SeedPlants(plant);

            var viewModel = await CreateInitializedViewModelAsync(plantRepository);
            viewModel.SelectedPlant = viewModel.Plants[0];

            var newRequirement = new SunlightRequirement { Hours = 6, Period = SunlightPeriod.Day };

            // When: the call should propagate the exception
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => viewModel.AddOrReplaceSunlightRequirementAsync(newRequirement));

            // Then: the plant's local state remains unchanged - still no Sunlight card
            var careStatuses = viewModel.CareStatuses.ToList();
            Assert.DoesNotContain(careStatuses, c => c is SunlightStatusViewModel);
        }

        // -- Notes Tests --

        [Fact]
        public async Task UpdatePlantNotesAsync_GivenNewNotes_PersistsAndUpdatesPlantObject()
        {
            // Given: a plant with existing notes
            var plant = new Plant { Name = "Aloe Vera", Notes = "Old notes" };

            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var viewModel = await CreateInitializedViewModelAsync(plantRepository);

            var selectedPlant = viewModel.Plants[0];

            // When: the notes are updated
            await viewModel.UpdatePlantNotesAsync(selectedPlant, "New notes");

            // Then: the text went through the repository and not only onto the
            // object - the ViewModel assigns it locally as well, so the call count
            // is what tells the two apart
            Assert.Equal(1, plantRepository.UpdatePlantNotesAsyncCallCount);
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

            var viewModel = await CreateInitializedViewModelAsync(plantRepository);

            var selectedPlant = viewModel.Plants[0];

            // When: the call should propagate the exception
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => viewModel.UpdatePlantNotesAsync(selectedPlant, "New notes"));

            // Then: the in-memory plant object was NOT updated
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
            var viewModel = await CreateInitializedViewModelAsync();

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
            var plant = AloeVera();
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var viewModel = await CreateInitializedViewModelAsync(plantRepository);
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
            var viewModel = await CreateInitializedViewModelAsync();

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
            var plant = AloeVera();
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var viewModel = await CreateInitializedViewModelAsync(plantRepository);
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
            var plant = AloeVera();
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var viewModel = await CreateInitializedViewModelAsync(plantRepository);
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
            var plant = AloeVera(WateringEveryDays(7));

            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var viewModel = await CreateInitializedViewModelAsync(plantRepository);
            viewModel.SelectedPlant = viewModel.Plants[0];

            var wateringCard = viewModel.CareStatuses.OfType<WateringStatusViewModel>().Single();

            (Plant plant, CareType care)? raisedArgs = null;
            int eventRaisedCount = 0;
            viewModel.EditScheduleRequested += (_, args) =>
            {
                eventRaisedCount++;
                raisedArgs = args;
            };

            // When: EditCommand is executed on the Watering card
            wateringCard.EditCommand!.Execute(null);

            // Then: EditScheduleRequested was raised exactly once, with the
            // selected plant and the care type "Watering" as the arguments
            Assert.Equal(1, eventRaisedCount);
            Assert.Same(viewModel.SelectedPlant, raisedArgs!.Value.plant);
            Assert.Equal(CareType.Watering, raisedArgs.Value.care);
        }

        // Helper method: Maps a simple string identifier to the actual command on the ViewModel -
        // necessary because [InlineData] can only carry constant values, not delegates or direct command references.
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
            var timerService = new FakeTimerService();

            // When: a MainViewModel is constructed with it
            CreateViewModel(timerService: timerService);

            // Then: Start was called with a 5-minute interval
            Assert.True(timerService.StartWasCalled);
            Assert.Equal(TimeSpan.FromMinutes(5), timerService.LastInterval);
        }

        [Fact]
        public async Task RefreshCareStatuses_WhenCalled_RaisesPropertyChangedForCareStatuses()
        {
            // Given: an initialized MainViewModel
            var viewModel = await CreateInitializedViewModelAsync();

            var raisedProperties = new List<string>();
            viewModel.PropertyChanged += (_, e) => raisedProperties.Add(e.PropertyName!);

            // When: RefreshCareStatuses is called directly
            viewModel.RefreshCareStatuses();

            // Then: PropertyChanged was raised for CareStatuses
            Assert.Contains(nameof(MainViewModel.CareStatuses), raisedProperties);
        }

        [Fact]
        public async Task SimulatedTimerTick_WhenTriggered_RaisesPropertyChangedForCareStatuses()
        {
            var timerService = new FakeTimerService();

            var viewModel = await CreateInitializedViewModelAsync(timerService: timerService);

            var raisedProperties = new List<string>();
            viewModel.PropertyChanged += (_, e) => raisedProperties.Add(e.PropertyName!);

            // When: a timer tick is simulated, without waiting for a real 5-minute interval
            timerService.TriggerTick();

            // Then: the callback passed to _timerService.Start(...) in the constructor is genuinely wired
            // to RefreshCareStatuses, and the timer actually triggers it
            Assert.Contains(nameof(MainViewModel.CareStatuses), raisedProperties);
        }

        [Fact]
        public async Task StopCareStatusRefreshTimer_WhenCalled_StopsTheTimerService()
        {
            // Given: an initialized MainViewModel
            var timerService = new FakeTimerService();

            var viewModel = await CreateInitializedViewModelAsync(timerService: timerService);

            // When: StopCareStatusRefreshTimer is called
            viewModel.StopCareStatusRefreshTimer();

            // Then: the underlying timer service was stopped
            Assert.True(timerService.StopWasCalled);
        }

        // -- Rename Plant Tests --

        [Fact]
        public async Task RenamePlantCommand_GivenPlantParameter_CanExecuteReturnsTrue()
        {
            // Given: an initialized MainViewModel and a plant
            var plant = AloeVera();
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var viewModel = await CreateInitializedViewModelAsync(plantRepository);

            // When: CanExecute is evaluated with a plant as parameter
            var canExecute = viewModel.RenamePlantCommand.CanExecute(viewModel.Plants[0]);

            // Then: the command is executable
            Assert.True(canExecute);
        }

        [Fact]
        public async Task RenamePlantCommand_GivenNullParameter_CanExecuteReturnsFalse()
        {
            // Given: an initialized MainViewModel
            var viewModel = await CreateInitializedViewModelAsync();

            // When: CanExecute is evaluated without a plant
            var canExecute = viewModel.RenamePlantCommand.CanExecute(null);

            // Then: there's nothing to rename, so the command is blocked
            Assert.False(canExecute);
        }

        [Fact]
        public async Task RenamePlantAsync_GivenRepositoryThrows_PropagatesExceptionAndKeepsOldName()
        {
            // Given: a plant and a repository configured to fail on rename
            var plant = AloeVera();
            var plantRepository = new FakePlantRepository { ShouldThrowOnRename = true };
            plantRepository.SeedPlants(plant);

            var viewModel = await CreateInitializedViewModelAsync(plantRepository);

            var selectedPlant = viewModel.Plants[0];

            // When: renaming is attempted
            var exception = await Record.ExceptionAsync(
                () => viewModel.RenamePlantAsync(selectedPlant, "Basil"));

            // Then: the exception is propagated to the caller (MainWindow catches it
            // and shows an error dialog), and the in-memory name stays untouched
            Assert.IsType<InvalidOperationException>(exception);
            Assert.Equal("Aloe Vera", selectedPlant.Name);
        }

        // -- Helpers --

        // Builds a ViewModel on the given fakes; the ones a test does not mention are fresh defaults.
        private static MainViewModel CreateViewModel(
            FakePlantRepository? plantRepository = null,
            FakeDialogService? dialogService = null,
            FakeTimerService? timerService = null)
        {
            return new MainViewModel(
                plantRepository ?? new FakePlantRepository(),
                dialogService ?? new FakeDialogService(),
                timerService ?? new FakeTimerService(),
                new FakeTimeProvider(Now),
                new ThemeViewModel(new FakeThemeService(), new FakeSettingsService()));
        }

        // Same, but with the plants of the repository already loaded.
        private static async Task<MainViewModel> CreateInitializedViewModelAsync(
            FakePlantRepository? plantRepository = null,
            FakeDialogService? dialogService = null,
            FakeTimerService? timerService = null)
        {
            var viewModel = CreateViewModel(plantRepository, dialogService, timerService);
            await viewModel.InitializeAsync();
            return viewModel;
        }
    }
}
