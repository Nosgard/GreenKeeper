using GreenKeeper.Tests.Fakes;
using GreenKeeper.ViewModels;
using static GreenKeeper.Tests.TestPlants;
using static GreenKeeper.Tests.ViewModels.MainViewModelTestHelpers;
using GreenKeeper.ViewModels.CareStatuses.Abstract;
using GreenKeeper.ViewModels.CareStatuses.Active;

namespace GreenKeeper.Tests.ViewModels
{
    public class MainViewModelTests
    {
        // -- Plant Selection Tests --

        [Fact]
        public async Task SelectedPlant_GivenPlantIsSelected_UpdatesIsPlantSelectedAndRaisesPropertyChanged()
        {
            // Given: an initialized MainViewModel with one plant, nothing selected yet
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(AloeVera());

            var viewModel = await CreateInitializedViewModelAsync(plantRepository);

            // Sanity check of the start state, before the When.
            Assert.False(viewModel.Dashboard.IsPlantSelected);

            var raisedProperties = new List<string>();
            viewModel.PlantList.PropertyChanged += (_, e) => raisedProperties.Add(e.PropertyName!);
            viewModel.Dashboard.PropertyChanged += (_, e) => raisedProperties.Add(e.PropertyName!);

            // When: a plant is selected
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];

            // Then: IsPlantSelected is true, and PropertyChanged was raised for SelectedPlant
            // and IsPlantSelected
            Assert.True(viewModel.Dashboard.IsPlantSelected);
            Assert.Contains(nameof(PlantListViewModel.SelectedPlant), raisedProperties);
            Assert.Contains(nameof(DashboardViewModel.IsPlantSelected), raisedProperties);
        }

        [Fact]
        public async Task SelectedPlant_GivenPlantIsSelected_RaisesSelectedPlantThenIsPlantSelectedAfterTheCardsAreBuilt()
        {
            // Given: an initialized MainViewModel with one plant, nothing selected yet
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(AloeVera());

            var viewModel = await CreateInitializedViewModelAsync(plantRepository);

            var raisedProperties = new List<string>();
            int? cardsWhenIsPlantSelectedWasRaised = null;
            viewModel.PlantList.PropertyChanged += (_, e) => raisedProperties.Add(e.PropertyName!);
            viewModel.Dashboard.PropertyChanged += (_, e) =>
            {
                raisedProperties.Add(e.PropertyName!);
                if (e.PropertyName == nameof(DashboardViewModel.IsPlantSelected))
                {
                    cardsWhenIsPlantSelectedWasRaised = viewModel.Dashboard.CareStatuses.Count();
                }
            };

            // When: a plant is selected
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];

            // Then: PropertyChanged was raised for SelectedPlant, then for IsPlantSelected with
            // the watering card already built, so the heading appears with the cards. None is
            // raised for CareStatuses: it stays the same ObservableCollection
            Assert.Equal(
                new[]
                {
                    nameof(PlantListViewModel.SelectedPlant),
                    nameof(DashboardViewModel.IsPlantSelected)
                },
                raisedProperties);
            Assert.Equal(1, cardsWhenIsPlantSelectedWasRaised);
        }

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

        // -- Periodic Refresh Tests --
        // A tick must leave untouched what did not change: a screen reader reads a rebuilt card
        // or sidebar entry out again.

        [Fact]
        public async Task TimerTick_WhenTriggered_ReachesTheCardsOfTheSelectedPlant()
        {
            // Given: a selected plant, on a timer service the test can trigger
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(AloeVera(WateringEveryDays(7)));
            var timerService = new FakeTimerService();
            var viewModel = await CreateInitializedViewModelAsync(plantRepository, timerService: timerService);
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];

            var card = viewModel.Dashboard.CareStatuses.Single();
            var raisedProperties = new List<string>();
            card.PropertyChanged += (_, e) => raisedProperties.Add(e.PropertyName!);

            // When: a timer tick is simulated, without waiting for a real 5-minute interval
            timerService.TriggerTick();

            // Then: the callback given to _timerService.Start in the constructor refreshes
            // the card instead of replacing it
            Assert.Contains(nameof(CareStatusViewModel.StatusText), raisedProperties);
        }

        [Fact]
        public async Task TimerTick_GivenNothingChanged_KeepsTheCards()
        {
            // Given: a selected plant whose watering is due in a week
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(AloeVera(WateringEveryDays(7, nextDueAt: Now.AddDays(7))));
            var timerService = new FakeTimerService();
            var viewModel = await CreateInitializedViewModelAsync(plantRepository, timerService: timerService);
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];
            var cardsBefore = viewModel.Dashboard.CareStatuses.ToList();

            // When: the periodic refresh runs
            timerService.TriggerTick();

            // Then: the same cards are still there
            Assert.Equal(cardsBefore, viewModel.Dashboard.CareStatuses.ToList());
        }

        [Fact]
        public async Task TimerTick_GivenTheDueDatePassed_UpdatesTheCardInPlace()
        {
            // Given: a watering card that is due today, on a clock the test moves on
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(AloeVera(WateringEveryDays(7, nextDueAt: Now)));
            var timeProvider = new FakeTimeProvider(Now);
            var timerService = new FakeTimerService();
            var viewModel = await CreateInitializedViewModelAsync(
                plantRepository, timerService: timerService, timeProvider: timeProvider);
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];

            var card = viewModel.Dashboard.CareStatuses.OfType<WateringStatusViewModel>().Single();
            var raisedProperties = new List<string>();
            card.PropertyChanged += (_, e) => raisedProperties.Add(e.PropertyName!);

            // When: a day passes and the periodic refresh runs
            timeProvider.Now = Now.AddDays(1);
            timerService.TriggerTick();

            // Then: the same card reports its new state, so the element a screen reader is
            // on stays the one it has announced
            Assert.Same(card, viewModel.Dashboard.CareStatuses.OfType<WateringStatusViewModel>().Single());
            Assert.Contains(nameof(WateringStatusViewModel.StatusText), raisedProperties);
            Assert.Contains(nameof(WateringStatusViewModel.IsOverdue), raisedProperties);
            Assert.Contains(nameof(WateringStatusViewModel.IsCompletable), raisedProperties);
            Assert.Equal("Overdue for 1 day", card.StatusText);
        }

        [Fact]
        public async Task TimerTick_GivenNothingChanged_LeavesTheSidebarAlone()
        {
            // Given: a plant whose watering is due in a week, so no state can change yet
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(AloeVera(WateringEveryDays(7, nextDueAt: Now.AddDays(7))));
            var timerService = new FakeTimerService();
            var viewModel = await CreateInitializedViewModelAsync(plantRepository, timerService: timerService);
            var sidebarRebuilds = SidebarRebuildsOf(viewModel);

            // When: the periodic refresh runs
            timerService.TriggerTick();

            // Then: the sidebar is not rebuilt
            Assert.Equal(0, sidebarRebuilds());
        }

        [Fact]
        public async Task TimerTick_GivenAPlantBecameOverdue_RebuildsTheSidebar()
        {
            // Given: a plant that is due today, on a clock the test moves on
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(AloeVera(WateringEveryDays(7, nextDueAt: Now)));
            var timeProvider = new FakeTimeProvider(Now);
            var timerService = new FakeTimerService();
            var viewModel = await CreateInitializedViewModelAsync(
                plantRepository, timerService: timerService, timeProvider: timeProvider);
            var sidebarRebuilds = SidebarRebuildsOf(viewModel);

            // When: a day passes, so the plant turns overdue, and the refresh runs
            timeProvider.Now = Now.AddDays(1);
            timerService.TriggerTick();

            // Then: the sidebar is rebuilt once, so the dot and the spoken name follow
            Assert.Equal(1, sidebarRebuilds());
        }
    }
}
