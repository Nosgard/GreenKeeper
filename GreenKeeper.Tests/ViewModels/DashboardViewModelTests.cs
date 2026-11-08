using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.Services;
using GreenKeeper.Tests.Fakes;
using GreenKeeper.ViewModels;
using static GreenKeeper.Tests.TestPlants;
using static GreenKeeper.Tests.ViewModels.MainViewModelTestHelpers;
using GreenKeeper.ViewModels.CareStatuses.Active;
using GreenKeeper.ViewModels.CareStatuses.Passive;
using System.Windows.Input;

namespace GreenKeeper.Tests.ViewModels
{
    /// <summary>
    /// The dashboard of the main window: the status cards of the selected plant and the
    /// actions on them. The tests build the whole MainViewModel, the way MainWindow gets it.
    /// </summary>
    public class DashboardViewModelTests
    {
        // -- Delete Plant --

        [Fact]
        public async Task DeletePlantCommand_GivenUserConfirms_RemovesPlantFromRepositoryAndCollection()
        {
            // Given: a repository with one plant, which is selected, and the user
            // confirming the deletion
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(AloeVera());

            var dialogService = new FakeDialogService { ConfirmResult = true };

            var viewModel = await CreateInitializedViewModelAsync(plantRepository, dialogService);
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];

            // When: DeletePlantCommand is executed
            await viewModel.Dashboard.DeletePlantCommand.ExecuteAsync(null);

            // Then: the plant is gone from both the UI collection and the repository and no plant remains selected
            Assert.Empty(viewModel.PlantList.Plants);
            Assert.Empty(await plantRepository.GetPlantsAsync());
            Assert.Null(viewModel.PlantList.SelectedPlant);
        }

        [Fact]
        public async Task DeletePlantCommand_GivenUserDeclines_KeepsPlantUnchanged()
        {
            // Given: a repository with one plant, which is selected, and the user
            // declining the deletion
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(AloeVera());

            var dialogService = new FakeDialogService { ConfirmResult = false };

            var viewModel = await CreateInitializedViewModelAsync(plantRepository, dialogService);
            var selectedPlant = viewModel.PlantList.Plants[0];
            viewModel.PlantList.SelectedPlant = selectedPlant;

            // When: DeletePlantCommand is executed
            await viewModel.Dashboard.DeletePlantCommand.ExecuteAsync(null);

            // Then: nothing changed; the plant remains in both the collection and the repository, and stays selected
            Assert.Single(viewModel.PlantList.Plants);
            Assert.Single(await plantRepository.GetPlantsAsync());
            Assert.Equal(selectedPlant, viewModel.PlantList.SelectedPlant);
        }

        [Fact]
        public async Task DeletePlantCommand_GivenRepositoryThrows_ShowsErrorAndKeepsPlant()
        {
            // Given: a repository configured to fail on delete, one plant selected, and the user confirming the deletion
            var plantRepository = new FakePlantRepository { ShouldThrowOnDelete = true };
            plantRepository.SeedPlants(AloeVera());

            var dialogService = new FakeDialogService { ConfirmResult = true };

            var viewModel = await CreateInitializedViewModelAsync(plantRepository, dialogService);
            var selectedPlant = viewModel.PlantList.Plants[0];
            viewModel.PlantList.SelectedPlant = selectedPlant;

            // When: DeletePlantCommand is executed
            await viewModel.Dashboard.DeletePlantCommand.ExecuteAsync(null);

            // Then: an error is shown, and the plant stays as it was, still in the UI collection and
            // still selected, since the deletion failed
            Assert.True(dialogService.ShowErrorWasCalled);
            Assert.Single(viewModel.PlantList.Plants);
            Assert.Equal(selectedPlant, viewModel.PlantList.SelectedPlant);
        }

        // -- CareStatuses Tests --

        [Fact]
        public async Task CareStatuses_GivenNoPlantSelected_ReturnsNoCards()
        {
            // Given: an initialized MainViewModel with a plant but none selected, as right after the start
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(AloeVera(WateringEveryDays(7)));

            var viewModel = await CreateInitializedViewModelAsync(plantRepository);

            // When: CareStatuses is read
            var careStatuses = viewModel.Dashboard.CareStatuses.ToList();

            // Then: the dashboard has no card to show
            Assert.Empty(careStatuses);
        }

        [Fact]
        public async Task CareStatuses_GivenPlantWithOnlyWatering_ReturnsOnlyWateringCard()
        {
            // Given: a plant with only a care schedule for Watering, no Fertilizing, no Sunlight
            var plant = PlantNamed("Cactus", WateringEveryDays(7));

            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var viewModel = await CreateInitializedViewModelAsync(plantRepository);
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];

            // When: CareStatuses is read
            var careStatuses = viewModel.Dashboard.CareStatuses.ToList();

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
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];

            // When: CareStatuses is read
            var careStatuses = viewModel.Dashboard.CareStatuses.ToList();

            // Then: three cards, Watering, Fertilizing and Sunlight, in that order
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
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];

            // When: CareStatuses is read
            var careStatuses = viewModel.Dashboard.CareStatuses.ToList();

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
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];

            var wateringCard = viewModel.Dashboard.CareStatuses.OfType<WateringStatusViewModel>().Single();

            // When: CompleteCommand is executed
            await wateringCard.CompleteCommand!.ExecuteAsync(null);

            // Then: the stored schedule is due exactly 7 days from now (NextDueAt) and was cared
            // for now (LastCaredAt), read from the repository rather than from the ViewModel
            var persistedSchedule = (await plantRepository.GetPlantsAsync())
                .Single()
                .CareSchedules
                .Single(s => s.Care == CareType.Watering);

            Assert.Equal(Now.AddDays(7), persistedSchedule.NextDueAt);
            Assert.Equal(Now, persistedSchedule.LastCaredAt);
        }

        [Fact]
        public async Task WateringCard_CompleteCommand_GivenValidSchedule_ShowsTheNewDueDateOnTheCard()
        {
            // Given: a plant with an overdue Watering schedule
            var plant = AloeVera(WateringEveryDays(7, nextDueAt: Now.AddDays(-1)));

            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var viewModel = await CreateInitializedViewModelAsync(plantRepository);
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];

            var wateringCard = viewModel.Dashboard.CareStatuses.OfType<WateringStatusViewModel>().Single();

            // When: CompleteCommand is executed
            await wateringCard.CompleteCommand!.ExecuteAsync(null);

            // Then: the card the dashboard shows afterwards reads the new due date and can no
            // longer be completed, without the plants being loaded again
            var updatedCard = viewModel.Dashboard.CareStatuses.OfType<WateringStatusViewModel>().Single();
            Assert.Equal("Due in 1 week", updatedCard.StatusText);
            Assert.False(updatedCard.IsCompletable);
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
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];

            var fertilizingCard = viewModel.Dashboard.CareStatuses.OfType<FertilizingStatusViewModel>().Single();

            // When: CompleteCommand is executed on the Fertilizing card
            await fertilizingCard.CompleteCommand!.ExecuteAsync(null);

            // Then: only the Fertilizing schedule was recalculated and stored; the due date of Watering is untouched
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
            // Given: a plant with a Watering schedule without IntervalAmount and IntervalUnit, but with a next due date (NextDueAt)
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
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];

            var wateringCard = viewModel.Dashboard.CareStatuses.OfType<WateringStatusViewModel>().Single();

            // When: CompleteCommand is executed
            await wateringCard.CompleteCommand!.ExecuteAsync(null);

            // Then: the repository was never called, and the due date is untouched
            Assert.Equal(0, plantRepository.CompleteCareScheduleAsyncCallCount);

            var persistedSchedule = (await plantRepository.GetPlantsAsync())
                .Single()
                .CareSchedules
                .Single();

            Assert.Equal(originalDueDate, persistedSchedule.NextDueAt);
        }

        [Fact]
        public async Task WateringCard_CompleteCommand_GivenRepositoryThrows_ShowsErrorAndKeepsDueDate()
        {
            // Given: a plant with an overdue Watering schedule, and the repository configured to fail on completing
            var originalDueDate = Now.AddDays(-1);
            var plant = AloeVera(WateringEveryDays(7, nextDueAt: originalDueDate));

            var plantRepository = new FakePlantRepository { ShouldThrowOnCompleteCareSchedule = true };
            plantRepository.SeedPlants(plant);

            var dialogService = new FakeDialogService();

            var viewModel = await CreateInitializedViewModelAsync(plantRepository, dialogService);
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];

            var wateringCard = viewModel.Dashboard.CareStatuses.OfType<WateringStatusViewModel>().Single();

            // When: CompleteCommand is executed
            await wateringCard.CompleteCommand!.ExecuteAsync(null);

            // Then: an error is shown, and the schedule stays overdue and uncared for; the card
            // must not show a new due date the database never took
            Assert.True(dialogService.ShowErrorWasCalled);

            var schedule = viewModel.PlantList.Plants[0].CareSchedules.Single();
            Assert.Equal(originalDueDate, schedule.NextDueAt);
            Assert.Null(schedule.LastCaredAt);
        }

        // -- Remove Button Tests --

        [Fact]
        public async Task FertilizingCard_RemoveCommand_GivenUserConfirms_RemovesFromRepositoryAndCareStatuses()
        {
            // Given: a plant with both Watering and Fertilizing schedules, and the
            // user confirming the removal
            var plant = AloeVera(WateringEveryDays(7), FertilizingEveryDays(30));

            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var dialogService = new FakeDialogService { ConfirmResult = true };

            var viewModel = await CreateInitializedViewModelAsync(plantRepository, dialogService);
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];

            var fertilizingCard = viewModel.Dashboard.CareStatuses.OfType<FertilizingStatusViewModel>();

            // When: RemoveCommand is executed on the Fertilizing card
            await fertilizingCard.Single().RemoveCommand!.ExecuteAsync(null);

            // Then: Fertilizing is gone from the repository and from the cards, while Watering stays
            var persistedSchedules = (await plantRepository.GetPlantsAsync()).Single().CareSchedules;
            Assert.DoesNotContain(persistedSchedules, s => s.Care == CareType.Fertilizing);
            Assert.Contains(persistedSchedules, s => s.Care == CareType.Watering);

            var updatedCareStatuses = viewModel.Dashboard.CareStatuses.ToList();
            Assert.DoesNotContain(updatedCareStatuses, c => c is FertilizingStatusViewModel);
            Assert.Contains(updatedCareStatuses, c => c is WateringStatusViewModel);
        }

        [Fact]
        public async Task FertilizingCard_RemoveCommand_GivenUserDeclines_KeepsScheduleUnchanged()
        {
            // Given: a plant with Watering and Fertilizing schedules, and the
            // user declining the removal
            var plant = AloeVera(WateringEveryDays(7), FertilizingEveryDays(30));

            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var dialogService = new FakeDialogService { ConfirmResult = false };

            var viewModel = await CreateInitializedViewModelAsync(plantRepository, dialogService);
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];

            var fertilizingCard = viewModel.Dashboard.CareStatuses.OfType<FertilizingStatusViewModel>().Single();

            // When: RemoveCommand is executed on the Fertilizing card
            await fertilizingCard.RemoveCommand!.ExecuteAsync(null);

            // Then: nothing changed; the Fertilizing card is still shown among CareStatuses
            var updatedCareStatuses = viewModel.Dashboard.CareStatuses.ToList();
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
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];

            var fertilizingCard = viewModel.Dashboard.CareStatuses.OfType<FertilizingStatusViewModel>().Single();

            // When: RemoveCommand is executed on the Fertilizing card
            await fertilizingCard.RemoveCommand!.ExecuteAsync(null);

            // Then: an error is shown, and the Fertilizing schedule stays both in the repository and among CareStatuses
            Assert.True(dialogService.ShowErrorWasCalled);

            var persistedSchedules = (await plantRepository.GetPlantsAsync()).Single().CareSchedules;
            Assert.Contains(persistedSchedules, s => s.Care == CareType.Fertilizing);

            var updatedCareStatuses = viewModel.Dashboard.CareStatuses.ToList();
            Assert.Contains(updatedCareStatuses, c => c is FertilizingStatusViewModel);
        }

        [Fact]
        public async Task SunlightCard_RemoveCommand_GivenUserConfirms_RemovesFromRepositoryAndCareStatuses()
        {
            // Given: a plant with a Watering schedule and a sunlight requirement,
            // and the user confirming the removal
            var plant = AloeVera(WateringEveryDays(7));
            plant.SunlightRequirement = DailySunlight(6);

            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var dialogService = new FakeDialogService { ConfirmResult = true };

            var viewModel = await CreateInitializedViewModelAsync(plantRepository, dialogService);
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];

            var sunlightCard = viewModel.Dashboard.CareStatuses.OfType<SunlightStatusViewModel>().Single();

            // When: RemoveCommand is executed on the Sunlight card
            await sunlightCard.RemoveCommand!.ExecuteAsync(null);

            // Then: the sunlight requirement is gone from the repository and from the cards, while Watering stays
            var persistedPlant = (await plantRepository.GetPlantsAsync()).Single();
            Assert.Null(persistedPlant.SunlightRequirement);
            Assert.Contains(viewModel.Dashboard.CareStatuses, s => s is WateringStatusViewModel);

            var updatedCareStatuses = viewModel.Dashboard.CareStatuses.ToList();
            Assert.DoesNotContain(updatedCareStatuses, c => c is SunlightStatusViewModel);
            Assert.Contains(updatedCareStatuses, c => c is WateringStatusViewModel);
        }

        [Fact]
        public async Task SunlightCard_RemoveCommand_GivenUserDeclines_KeepsRequirementUnchanged()
        {
            // Given: a plant with a Watering schedule and a sunlight requirement,
            // and the user declining the removal
            var plant = AloeVera(WateringEveryDays(7));
            plant.SunlightRequirement = DailySunlight(6);

            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var dialogService = new FakeDialogService { ConfirmResult = false };

            var viewModel = await CreateInitializedViewModelAsync(plantRepository, dialogService);
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];

            var sunlightCard = viewModel.Dashboard.CareStatuses.OfType<SunlightStatusViewModel>().Single();

            // When: RemoveCommand is executed on the Sunlight card
            await sunlightCard.RemoveCommand!.ExecuteAsync(null);

            // Then: nothing changed; the sunlight requirement remains in the repository and its card is still shown among CareStatuses
            var persistedPlant = (await plantRepository.GetPlantsAsync()).Single();
            Assert.NotNull(persistedPlant.SunlightRequirement);

            var updatedCareStatuses = viewModel.Dashboard.CareStatuses.ToList();
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
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];

            var sunlightCard = viewModel.Dashboard.CareStatuses.OfType<SunlightStatusViewModel>().Single();

            // When: RemoveCommand is executed on the Sunlight card
            await sunlightCard.RemoveCommand!.ExecuteAsync(null);

            // Then: an error is shown, and the sunlight requirement stays both in the repository and among CareStatuses
            Assert.True(dialogService.ShowErrorWasCalled);

            var persistedPlant = (await plantRepository.GetPlantsAsync()).Single();
            Assert.NotNull(persistedPlant.SunlightRequirement);

            var updatedCareStatuses = viewModel.Dashboard.CareStatuses.ToList();
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
            Assert.Null(viewModel.PlantList.SelectedPlant);

            var newSchedule = new CareSchedule
            {
                Care = CareType.Fertilizing,
                IntervalAmount = 30,
                IntervalUnit = TimeUnit.Days
            };

            // When: AddOrReplaceCareScheduleAsync is called without a selected plant
            await viewModel.Dashboard.AddOrReplaceCareScheduleAsync(newSchedule);

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
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];

            var incompleteSchedule = new CareSchedule
            {
                Care = CareType.Fertilizing,
                IntervalAmount = null,
                IntervalUnit = null
            };

            // When: AddOrReplaceCareScheduleAsync is called with incomplete data
            await viewModel.Dashboard.AddOrReplaceCareScheduleAsync(incompleteSchedule);

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
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];

            var newFertilizingSchedule = new CareSchedule
            {
                Care = CareType.Fertilizing,
                IntervalAmount = 30,
                IntervalUnit = TimeUnit.Days
            };

            // When: the new Fertilizing schedule is added
            await viewModel.Dashboard.AddOrReplaceCareScheduleAsync(newFertilizingSchedule);

            // Then: it was stored with the calculated due date and now appears among CareStatuses, next to the existing Watering card
            var persistedFertilizing = (await plantRepository.GetPlantsAsync())
                .Single()
                .CareSchedules
                .Single(s => s.Care == CareType.Fertilizing);

            Assert.Equal(Now.AddDays(30), persistedFertilizing.NextDueAt);
            Assert.Equal(Now, persistedFertilizing.LastCaredAt);

            var careStatuses = viewModel.Dashboard.CareStatuses.ToList();
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
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];

            var replacementSchedule = new CareSchedule
            {
                Care = CareType.Fertilizing,
                IntervalAmount = 14,
                IntervalUnit = TimeUnit.Days
            };

            // When: the Fertilizing schedule is replaced
            await viewModel.Dashboard.AddOrReplaceCareScheduleAsync(replacementSchedule);

            // Then: one Fertilizing entry remains, with the new interval and no duplicate; Watering is untouched and CareStatuses shows two cards
            var persistedSchedules = (await plantRepository.GetPlantsAsync()).Single().CareSchedules;
            var persistedFertilizing = persistedSchedules.Where(s => s.Care == CareType.Fertilizing).Single();

            Assert.Equal(14, persistedFertilizing.IntervalAmount);
            Assert.Equal(Now.AddDays(14), persistedFertilizing.NextDueAt);

            Assert.Single(persistedSchedules, s => s.Care == CareType.Watering);
            Assert.Equal(2, viewModel.Dashboard.CareStatuses.Count());
        }

        [Fact]
        public async Task AddOrReplaceCareScheduleAsync_GivenExistingCareType_ShowsTheNewDueDateOnTheCard()
        {
            // Given: a selected plant that is fertilized every 30 days and next due in 30 days
            var plant = AloeVera(WateringEveryDays(7), FertilizingEveryDays(30, nextDueAt: Now.AddDays(30)));

            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var viewModel = await CreateInitializedViewModelAsync(plantRepository);
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];

            // When: the Fertilizing schedule is replaced by one with a 14-day interval
            await viewModel.Dashboard.AddOrReplaceCareScheduleAsync(FertilizingEveryDays(14));

            // Then: the Fertilizing card reads the due date of the new schedule; the old schedule
            // has left the plant object of the ViewModel, not only the repository
            var fertilizingCard = viewModel.Dashboard.CareStatuses.OfType<FertilizingStatusViewModel>().Single();
            Assert.Equal("Due in 2 weeks", fertilizingCard.StatusText);
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
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];

            var newFertilizingSchedule = new CareSchedule
            {
                Care = CareType.Fertilizing,
                IntervalAmount = 30,
                IntervalUnit = TimeUnit.Days
            };

            // When: the new Fertilizing schedule is added
            var exception = await Record.ExceptionAsync(() => viewModel.Dashboard.AddOrReplaceCareScheduleAsync(newFertilizingSchedule));

            // Then: the exception is propagated, and the plant's local state is unchanged: still no Fertilizing card
            Assert.IsType<InvalidOperationException>(exception);

            var careStatuses = viewModel.Dashboard.CareStatuses.ToList();
            Assert.DoesNotContain(careStatuses, c => c is FertilizingStatusViewModel);
        }

        [Fact]
        public async Task AddOrReplaceSunlightRequirementAsync_GivenNoSelectedPlant_DoesNothing()
        {
            // Given: an initialized MainViewModel with no plant selected
            var plantRepository = new FakePlantRepository();
            var viewModel = await CreateInitializedViewModelAsync(plantRepository);

            // Sanity check: nothing selected.
            Assert.Null(viewModel.PlantList.SelectedPlant);

            var newRequirement = new SunlightRequirement
            {
                Hours = 6,
                Period = SunlightPeriod.Day
            };
            // When: AddOrReplaceSunlightRequirementAsync is called without a selected plant
            await viewModel.Dashboard.AddOrReplaceSunlightRequirementAsync(newRequirement);

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
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];

            var newRequirement = new SunlightRequirement
            {
                Hours = 6,
                Period = SunlightPeriod.Day
            };

            // When: the new sunlight requirement is added
            await viewModel.Dashboard.AddOrReplaceSunlightRequirementAsync(newRequirement);

            // Then: it was stored once with the entered hours and period and now appears among CareStatuses, next to the existing Watering card
            Assert.Equal(1, plantRepository.AddOrReplaceSunlightRequirementAsyncCallCount);

            var persistedRequirement = (await plantRepository.GetPlantsAsync()).Single().SunlightRequirement;

            Assert.NotNull(persistedRequirement);
            Assert.Equal(6, persistedRequirement!.Hours);
            Assert.Equal(SunlightPeriod.Day, persistedRequirement.Period);

            var careStatuses = viewModel.Dashboard.CareStatuses.ToList();
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
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];

            var replacementRequirement = new SunlightRequirement { Hours = 3, Period = SunlightPeriod.Week };

            // When: the sunlight requirement is replaced
            await viewModel.Dashboard.AddOrReplaceSunlightRequirementAsync(replacementRequirement);

            // Then: the stored requirement has the new values, and CareStatuses still shows two cards (Watering + Sunlight)
            var persistedPlant = (await plantRepository.GetPlantsAsync()).Single();
            Assert.NotNull(persistedPlant.SunlightRequirement);
            Assert.Equal(3, persistedPlant.SunlightRequirement!.Hours);
            Assert.Equal(SunlightPeriod.Week, persistedPlant.SunlightRequirement.Period);

            Assert.Equal(2, viewModel.Dashboard.CareStatuses.Count());
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
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];

            var newRequirement = new SunlightRequirement { Hours = 6, Period = SunlightPeriod.Day };

            // When: the new sunlight requirement is added
            var exception = await Record.ExceptionAsync(
                () => viewModel.Dashboard.AddOrReplaceSunlightRequirementAsync(newRequirement));

            // Then: the exception is propagated, and the plant's local state is unchanged: still no Sunlight card
            Assert.IsType<InvalidOperationException>(exception);

            var careStatuses = viewModel.Dashboard.CareStatuses.ToList();
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

            var selectedPlant = viewModel.PlantList.Plants[0];

            // When: the notes are updated
            await viewModel.Dashboard.UpdatePlantNotesAsync(selectedPlant, "New notes");

            // Then: the notes are stored in the repository, and the plant object
            // of the ViewModel carries them as well
            var persistedPlant = (await plantRepository.GetPlantsAsync()).Single();
            Assert.Equal("New notes", persistedPlant.Notes);
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

            var selectedPlant = viewModel.PlantList.Plants[0];

            // When: the notes are updated
            var exception = await Record.ExceptionAsync(
                () => viewModel.Dashboard.UpdatePlantNotesAsync(selectedPlant, "New notes"));

            // Then: the exception is propagated, and the in-memory plant keeps its old notes
            Assert.IsType<InvalidOperationException>(exception);
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

            // When: the command is looked up
            var command = GetCommand(viewModel, commandName);

            // Then: CanExecute is false, since no plant is selected
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
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];

            // When: the command is looked up
            var command = GetCommand(viewModel, commandName);

            // Then: CanExecute is true, since a plant is selected
            Assert.True(command.CanExecute(null));
        }

        [Fact]
        public async Task AddScheduleCommand_Execute_RaisesAddScheduleRequestedWithSelectedPlant()
        {
            // Given: an initialized MainViewModel with a plant selected
            var plant = AloeVera();
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var viewModel = await CreateInitializedViewModelAsync(plantRepository);
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];

            Plant? raisedPlant = null;
            int eventRaisedCount = 0;
            viewModel.Dashboard.AddScheduleRequested += (_, raisedArg) =>
            {
                eventRaisedCount++;
                raisedPlant = raisedArg;
            };

            // When: AddScheduleCommand is executed
            viewModel.Dashboard.AddScheduleCommand.Execute(null);

            // Then: AddScheduleRequested was raised exactly once, with the
            // selected plant as the argument
            Assert.Equal(1, eventRaisedCount);
            Assert.Same(viewModel.PlantList.SelectedPlant, raisedPlant);
        }

        [Fact]
        public async Task OpenNotesCommand_Execute_RaisesOpenNotesRequestedWithSelectedPlant()
        {
            // Given: an initialized MainViewModel with a plant selected
            var plant = AloeVera();
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var viewModel = await CreateInitializedViewModelAsync(plantRepository);
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];

            Plant? raisedPlant = null;
            int eventRaisedCount = 0;
            viewModel.Dashboard.OpenNotesRequested += (_, raisedArg) =>
            {
                eventRaisedCount++;
                raisedPlant = raisedArg;
            };

            // When: OpenNotesCommand is executed
            viewModel.Dashboard.OpenNotesCommand.Execute(null);

            // Then: OpenNotesRequested was raised exactly once, with the
            // selected plant as the argument
            Assert.Equal(1, eventRaisedCount);
            Assert.Same(viewModel.PlantList.SelectedPlant, raisedPlant);
        }

        [Theory]
        [InlineData(CareType.Watering)]
        [InlineData(CareType.Fertilizing)]
        [InlineData(CareType.Sunlight)]
        public async Task StatusCard_EditCommand_RaisesEditScheduleRequestedWithPlantAndItsCareType(CareType careType)
        {
            // Given: an initialized MainViewModel with a plant that shows all three status cards
            var plant = AloeVera(WateringEveryDays(7), FertilizingEveryDays(30));
            plant.SunlightRequirement = DailySunlight(6);

            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);

            var viewModel = await CreateInitializedViewModelAsync(plantRepository);
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];

            var card = viewModel.Dashboard.CareStatuses.Single(c => c.Care == careType);

            (Plant plant, CareType care)? raisedArgs = null;
            int eventRaisedCount = 0;
            viewModel.Dashboard.EditScheduleRequested += (_, args) =>
            {
                eventRaisedCount++;
                raisedArgs = args;
            };

            // When: EditCommand is executed on the card of the given care type
            card.EditCommand!.Execute(null);

            // Then: EditScheduleRequested was raised exactly once, with the
            // selected plant and the care type of that card as the arguments
            Assert.Equal(1, eventRaisedCount);
            Assert.Same(viewModel.PlantList.SelectedPlant, raisedArgs!.Value.plant);
            Assert.Equal(careType, raisedArgs.Value.care);
        }

        // Maps the name from [InlineData] to the command.
        private static ICommand GetCommand(MainViewModel viewModel, string commandName) => commandName switch
        {
            "AddSchedule" => viewModel.Dashboard.AddScheduleCommand,
            "DeletePlant" => viewModel.Dashboard.DeletePlantCommand,
            "OpenNotes" => viewModel.Dashboard.OpenNotesCommand,
            _ => throw new ArgumentOutOfRangeException(nameof(commandName))
        };

        // -- Announcement Tests --
        // What a screen reader says after an action: the result is otherwise only visible.

        [Fact]
        public async Task WateringCard_CompleteCommand_GivenValidSchedule_AnnouncesTheCareAndItsNewStatus()
        {
            // Given: a selected plant with an overdue watering, and a listener for announcements
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(AloeVera(WateringEveryDays(7, nextDueAt: Now.AddDays(-1))));
            var viewModel = await CreateInitializedViewModelAsync(plantRepository);
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];
            var announcements = AnnouncementsOf(viewModel);

            // When: the watering is completed
            await viewModel.Dashboard.CareStatuses.OfType<WateringStatusViewModel>().Single().CompleteCommand!.ExecuteAsync(null);

            // Then: the announcement names the care schedule and what its card reads now
            Assert.Equal(new[] { "Watering completed. Due in 1 week" }, announcements);
        }

        [Fact]
        public async Task WateringCard_CompleteCommand_GivenRepositoryThrows_AnnouncesNothing()
        {
            // Given: a repository that fails to store the completion
            var plantRepository = new FakePlantRepository { ShouldThrowOnCompleteCareSchedule = true };
            plantRepository.SeedPlants(AloeVera(WateringEveryDays(7, nextDueAt: Now.AddDays(-1))));
            var viewModel = await CreateInitializedViewModelAsync(plantRepository);
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];
            var announcements = AnnouncementsOf(viewModel);

            // When: the watering is completed
            await viewModel.Dashboard.CareStatuses.OfType<WateringStatusViewModel>().Single().CompleteCommand!.ExecuteAsync(null);

            // Then: no announcement; the error dialog already says what happened
            Assert.Empty(announcements);
        }

        [Theory]
        [InlineData(true, new[] { "Fertilizing schedule removed" })]
        [InlineData(false, new string[0])]
        public async Task FertilizingCard_RemoveCommand_GivenUsersAnswer_AnnouncesOnlyAConfirmedRemoval(bool isConfirmed, string[] expected)
        {
            // Given: a selected plant with a fertilizing schedule, and the user's answer to the question
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(AloeVera(WateringEveryDays(7), FertilizingEveryDays(30)));
            var viewModel = await CreateInitializedViewModelAsync(plantRepository, new FakeDialogService { ConfirmResult = isConfirmed });
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];
            var announcements = AnnouncementsOf(viewModel);

            // When: the fertilizing schedule is removed
            await viewModel.Dashboard.CareStatuses.OfType<FertilizingStatusViewModel>().Single().RemoveCommand!.ExecuteAsync(null);

            // Then: the removal is announced once it happened, and not when it was declined
            Assert.Equal(expected, announcements);
        }

        [Fact]
        public async Task SunlightCard_RemoveCommand_GivenUserConfirms_AnnouncesTheRemoval()
        {
            // Given: a selected plant with a sunlight requirement, and a user who confirms
            var plantRepository = new FakePlantRepository();
            var plant = AloeVera(WateringEveryDays(7));
            plant.SunlightRequirement = DailySunlight(6);
            plantRepository.SeedPlants(plant);
            var viewModel = await CreateInitializedViewModelAsync(plantRepository, new FakeDialogService { ConfirmResult = true });
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];
            var announcements = AnnouncementsOf(viewModel);

            // When: the sunlight requirement is removed
            await viewModel.Dashboard.CareStatuses.OfType<SunlightStatusViewModel>().Single().RemoveCommand!.ExecuteAsync(null);

            // Then: the removal is announced
            Assert.Equal(new[] { "Sunlight requirement removed" }, announcements);
        }

        [Fact]
        public async Task AddOrReplaceCareScheduleAsync_GivenSelectedPlant_AnnouncesTheScheduleAndItsStatus()
        {
            // Given: a selected plant without a fertilizing schedule
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(AloeVera(WateringEveryDays(7)));
            var viewModel = await CreateInitializedViewModelAsync(plantRepository);
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];
            var announcements = AnnouncementsOf(viewModel);

            // When: a fertilizing schedule of 14 days is saved
            await viewModel.Dashboard.AddOrReplaceCareScheduleAsync(FertilizingEveryDays(14));

            // Then: the announcement names the schedule and what its new card reads
            Assert.Equal(new[] { "Fertilizing schedule saved. Due in 2 weeks" }, announcements);
        }

        [Fact]
        public async Task AddOrReplaceSunlightRequirementAsync_GivenSelectedPlant_AnnouncesTheRequirementInWords()
        {
            // Given: a selected plant without a sunlight requirement
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(AloeVera(WateringEveryDays(7)));
            var viewModel = await CreateInitializedViewModelAsync(plantRepository);
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];
            var announcements = AnnouncementsOf(viewModel);

            // When: a requirement of 6 hours per day is saved
            await viewModel.Dashboard.AddOrReplaceSunlightRequirementAsync(new SunlightRequirement { Hours = 6, Period = SunlightPeriod.Day });

            // Then: the announcement writes the requirement out instead of reading "6h / day"
            Assert.Equal(new[] { "Sunlight requirement saved. 6 hours per day" }, announcements);
        }

        [Fact]
        public async Task DeletePlantCommand_GivenUserConfirms_AnnouncesTheDeletedPlant()
        {
            // Given: a selected plant, a user who confirms the question, and a listener for announcements
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(AloeVera());
            var viewModel = await CreateInitializedViewModelAsync(plantRepository, new FakeDialogService { ConfirmResult = true });
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];
            var announcements = AnnouncementsOf(viewModel);

            // When: the plant is deleted
            await viewModel.Dashboard.DeletePlantCommand.ExecuteAsync(null);

            // Then: the announcement names the deleted plant
            Assert.Equal(new[] { "Aloe Vera deleted" }, announcements);
        }

        // -- Safe Answer Tests --
        // The window offers the safe answer first, so pressing Enter right away removes nothing.

        [Fact]
        public async Task DeletePlantCommand_GivenTheQuestion_OffersNoFirst()
        {
            // Given: a selected plant
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(AloeVera());
            var dialogService = new FakeDialogService { ConfirmResult = false };
            var viewModel = await CreateInitializedViewModelAsync(plantRepository, dialogService);
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];

            // When: the plant is about to be deleted
            await viewModel.Dashboard.DeletePlantCommand.ExecuteAsync(null);

            // Then: "No" keeps the plant, so it is offered first
            Assert.Equal(Answer.No, dialogService.OfferedAnswer);
        }

        [Fact]
        public async Task FertilizingCard_RemoveCommand_GivenTheQuestion_OffersNoFirst()
        {
            // Given: a selected plant with a fertilizing schedule
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(AloeVera(WateringEveryDays(7), FertilizingEveryDays(30)));
            var dialogService = new FakeDialogService { ConfirmResult = false };
            var viewModel = await CreateInitializedViewModelAsync(plantRepository, dialogService);
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];

            // When: the schedule is about to be removed
            await viewModel.Dashboard.CareStatuses.OfType<FertilizingStatusViewModel>().Single().RemoveCommand!.ExecuteAsync(null);

            // Then: "No" keeps the schedule, so it is offered first
            Assert.Equal(Answer.No, dialogService.OfferedAnswer);
        }

        [Fact]
        public async Task SunlightCard_RemoveCommand_GivenTheQuestion_OffersNoFirst()
        {
            // Given: a selected plant with a sunlight requirement
            var plant = AloeVera(WateringEveryDays(7));
            plant.SunlightRequirement = DailySunlight(6);
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);
            var dialogService = new FakeDialogService { ConfirmResult = false };
            var viewModel = await CreateInitializedViewModelAsync(plantRepository, dialogService);
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];

            // When: the requirement is about to be removed
            await viewModel.Dashboard.CareStatuses.OfType<SunlightStatusViewModel>().Single().RemoveCommand!.ExecuteAsync(null);

            // Then: "No" keeps the requirement, so it is offered first
            Assert.Equal(Answer.No, dialogService.OfferedAnswer);
        }

        // -- Periodic Refresh Tests --

        [Fact]
        public async Task CareStatuses_ReadTwice_HandsOutTheSameCards()
        {
            // Given: a selected plant with all three cards
            var plant = AloeVera(WateringEveryDays(7), FertilizingEveryDays(30));
            plant.SunlightRequirement = DailySunlight(6);
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(plant);
            var viewModel = await CreateInitializedViewModelAsync(plantRepository);
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];

            // When: the cards are read twice
            var first = viewModel.Dashboard.CareStatuses.ToList();
            var second = viewModel.Dashboard.CareStatuses.ToList();

            // Then: the same card objects; the cards do not override Equals, so the check passes
            // only for the same objects. With new objects, a screen reader would announce the
            // cards again
            Assert.Equal(first, second);
        }

        [Fact]
        public async Task AddOrReplaceCareScheduleAsync_GivenAReplacedSchedule_GivesTheCardTheNewSchedule()
        {
            // Given: a selected plant whose watering is due in a week
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(AloeVera(WateringEveryDays(7, nextDueAt: Now.AddDays(7))));
            var viewModel = await CreateInitializedViewModelAsync(plantRepository);
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];

            // When: the watering interval is replaced by one of a single day
            await viewModel.Dashboard.AddOrReplaceCareScheduleAsync(WateringEveryDays(1));

            // Then: the card reads the new schedule, not the replaced one it was built on.
            // Unlike a tick, changed data must still rebuild the cards
            var card = viewModel.Dashboard.CareStatuses.OfType<WateringStatusViewModel>().Single();
            Assert.Equal("Due in 1 day", card.StatusText);
        }
    }
}
