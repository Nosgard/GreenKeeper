using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.Tests.Fakes;
using GreenKeeper.ViewModels;
using static GreenKeeper.Tests.TestPlants;
using static GreenKeeper.Tests.ViewModels.MainViewModelTestHelpers;

namespace GreenKeeper.Tests.ViewModels
{
    /// <summary>
    /// The sidebar of the main window: loading, search, adding and renaming. The tests
    /// build the whole MainViewModel, the way MainWindow gets it.
    /// </summary>
    public class PlantListViewModelTests
    {
        // -- Basic Tests --

        [Fact]
        public async Task InitializeAsync_GivenRepositoryWithOnePlant_PopulatesPlants()
        {
            // Given: a repository seeded with one existing plant
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(AloeVera());

            var viewModel = CreateViewModel(plantRepository);

            // When: InitializeAsync is called
            await viewModel.PlantList.InitializeAsync();

            // Then: Plants should contain exactly the seeded plant
            Assert.Single(viewModel.PlantList.Plants);
            Assert.Equal("Aloe Vera", viewModel.PlantList.Plants[0].Name);
        }

        [Fact]
        public async Task InitializeAsync_GivenRepositoryWithoutPlants_ReportsTheListAsLoaded()
        {
            // Given: a repository without plants, so the list stays empty after loading
            var viewModel = CreateViewModel(new FakePlantRepository());

            var raisedProperties = new List<string>();
            viewModel.PlantList.PropertyChanged += (_, e) => raisedProperties.Add(e.PropertyName!);

            // When: InitializeAsync is called
            await viewModel.PlantList.InitializeAsync();

            // Then: the empty list counts as loaded, and the view hears about it
            Assert.True(viewModel.PlantList.HasLoadedPlants);
            Assert.Contains(nameof(PlantListViewModel.HasLoadedPlants), raisedProperties);
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
            await viewModel.PlantList.AddPlantAsync(new Plant { Name = "Basil" });

            // Then: both plants should be present, not just the new one
            Assert.Equal(2, viewModel.PlantList.Plants.Count);
            Assert.Contains(viewModel.PlantList.Plants, p => p.Name == "Aloe Vera");
            Assert.Contains(viewModel.PlantList.Plants, p => p.Name == "Basil");
        }

        [Fact]
        public async Task AddPlantAsync_GivenRepositoryThrows_PropagatesExceptionAndDoesNotAddPlant()
        {
            // Given: a repository configured to fail when saving
            var plantRepository = new FakePlantRepository { ShouldThrowOnAdd = true };
            var viewModel = CreateViewModel(plantRepository);

            await viewModel.PlantList.InitializeAsync();

            // When: a plant is added
            var exception = await Record.ExceptionAsync(() => viewModel.PlantList.AddPlantAsync(new Plant { Name = "Basil" }));

            // Then: the exception is propagated and the plant does NOT appear in the UI-bound collection
            Assert.IsType<InvalidOperationException>(exception);
            Assert.Empty(viewModel.PlantList.Plants);
        }

        [Fact]
        public async Task AddPlantAsync_GivenPlantWithSchedules_StartsTheirIntervalsNow()
        {
            // Given: a plant from the wizard, watered every 7 days and fertilized every 30 days, without due dates yet
            var plantRepository = new FakePlantRepository();
            var viewModel = await CreateInitializedViewModelAsync(plantRepository);
            var plant = AloeVera(WateringEveryDays(7), FertilizingEveryDays(30));

            // When: the plant is added
            await viewModel.PlantList.AddPlantAsync(plant);

            // Then: every schedule counts as cared for now and is due one interval later
            var persisted = (await plantRepository.GetPlantsAsync()).Single();
            var watering = persisted.CareSchedules.Single(s => s.Care == CareType.Watering);
            var fertilizing = persisted.CareSchedules.Single(s => s.Care == CareType.Fertilizing);
            Assert.Equal(Now.AddDays(7), watering.NextDueAt);
            Assert.Equal(Now.AddDays(30), fertilizing.NextDueAt);
            Assert.Equal(Now, watering.LastCaredAt);
            Assert.Equal(Now, fertilizing.LastCaredAt);
        }

        // -- Search Tests --

        [Fact]
        public async Task SearchText_GivenPlantIsSelected_ResetsSelectedPlantToNull()
        {
            // Given: an initialized MainViewModel with a plant currently selected
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(AloeVera());

            var viewModel = await CreateInitializedViewModelAsync(plantRepository);
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];

            // Sanity check before the actual "When".
            Assert.NotNull(viewModel.PlantList.SelectedPlant);

            // When: the search text changes
            viewModel.PlantList.SearchText = "al";

            // Then: the previously selected plant is deselected
            Assert.Null(viewModel.PlantList.SelectedPlant);
        }

        [Fact]
        public async Task SearchText_GivenSameValueIsSetAgain_DoesNotResetSelectedPlant()
        {
            // Given: an initialized MainViewModel with a plant selected, and
            // SearchText already set to a specific value
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(AloeVera());

            var viewModel = await CreateInitializedViewModelAsync(plantRepository);
            viewModel.PlantList.SearchText = "al";
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];

            // When: SearchText is set to the exact same value again
            viewModel.PlantList.SearchText = "al";

            // Then: nothing actually changed, so the selection should be preserved
            // (the setter's early-return guard for unchanged values should prevent
            // the deselection logic from running again)
            Assert.NotNull(viewModel.PlantList.SelectedPlant);
        }

        [Fact]
        public async Task SearchText_GivenPartOfAName_ShowsOnlyTheMatchingPlantsIgnoringCase()
        {
            // Given: an initialized MainViewModel with two lilies and a basil
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(PlantNamed("Peace Lily"), PlantNamed("Basil"), PlantNamed("Calla Lily"));

            var viewModel = await CreateInitializedViewModelAsync(plantRepository);

            // When: "LILY" is typed into the search box
            viewModel.PlantList.SearchText = "LILY";

            // Then: the sidebar lists both lilies despite the different casing, and hides the basil
            Assert.Equal(new[] { "Peace Lily", "Calla Lily" }, VisiblePlantNames(viewModel));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task SearchText_GivenSearchIsEmptiedOrOnlyWhitespace_ShowsAllPlantsAgain(string blankSearchText)
        {
            // Given: an initialized MainViewModel whose sidebar is filtered down to the basil
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(PlantNamed("Peace Lily"), PlantNamed("Basil"), PlantNamed("Calla Lily"));

            var viewModel = await CreateInitializedViewModelAsync(plantRepository);
            viewModel.PlantList.SearchText = "basil";

            // When: the search box is emptied or left with nothing but spaces
            viewModel.PlantList.SearchText = blankSearchText;

            // Then: the sidebar lists every plant again
            Assert.Equal(new[] { "Peace Lily", "Basil", "Calla Lily" }, VisiblePlantNames(viewModel));
        }

        // -- CanExecute & Event Tests --

        [Fact]
        public async Task AddPlantCommand_Execute_RaisesAddPlantRequested()
        {
            // Given: an initialized MainViewModel
            var viewModel = await CreateInitializedViewModelAsync();

            int eventRaisedCount = 0;
            viewModel.PlantList.AddPlantRequested += (_, _) => eventRaisedCount++;

            // When: AddPlantCommand is executed
            viewModel.PlantList.AddPlantCommand.Execute(null);

            // Then: AddPlantRequested was raised exactly once
            Assert.Equal(1, eventRaisedCount);
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
            var canExecute = viewModel.PlantList.RenamePlantCommand.CanExecute(viewModel.PlantList.Plants[0]);

            // Then: the command is executable
            Assert.True(canExecute);
        }

        [Fact]
        public async Task RenamePlantCommand_GivenNullParameter_CanExecuteReturnsFalse()
        {
            // Given: an initialized MainViewModel
            var viewModel = await CreateInitializedViewModelAsync();

            // When: CanExecute is evaluated without a plant
            var canExecute = viewModel.PlantList.RenamePlantCommand.CanExecute(null);

            // Then: there's nothing to rename, so the command is blocked
            Assert.False(canExecute);
        }

        [Fact]
        public async Task RenamePlantCommand_Execute_RaisesRenamePlantRequestedWithThePlantFromTheParameter()
        {
            // Given: an initialized MainViewModel with two plants, the first one selected
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(AloeVera(), PlantNamed("Basil"));

            var viewModel = await CreateInitializedViewModelAsync(plantRepository);
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];
            var rightClickedPlant = viewModel.PlantList.Plants[1];

            Plant? raisedPlant = null;
            int eventRaisedCount = 0;
            viewModel.PlantList.RenamePlantRequested += (_, raisedArg) =>
            {
                eventRaisedCount++;
                raisedPlant = raisedArg;
            };

            // When: RenamePlantCommand is executed from the context menu of the other plant
            viewModel.PlantList.RenamePlantCommand.Execute(rightClickedPlant);

            // Then: RenamePlantRequested was raised exactly once, with the plant
            // that was right-clicked - not with the selected one
            Assert.Equal(1, eventRaisedCount);
            Assert.Same(rightClickedPlant, raisedPlant);
        }

        [Fact]
        public async Task RenamePlantAsync_GivenNewName_PersistsAndUpdatesPlantObject()
        {
            // Given: a plant named "Aloe Vera"
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(AloeVera());

            var viewModel = await CreateInitializedViewModelAsync(plantRepository);

            var plant = viewModel.PlantList.Plants[0];

            // When: the plant is renamed
            await viewModel.PlantList.RenamePlantAsync(plant, "Basil");

            // Then: the new name is stored in the repository, and the plant object
            // of the ViewModel carries it as well
            var persistedPlant = (await plantRepository.GetPlantsAsync()).Single();
            Assert.Equal("Basil", persistedPlant.Name);
            Assert.Equal("Basil", plant.Name);
        }

        [Fact]
        public async Task RenamePlantAsync_GivenSelectedPlant_RaisesPropertyChangedForSelectedPlant()
        {
            // Given: an initialized MainViewModel with its only plant selected
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(AloeVera());

            var viewModel = await CreateInitializedViewModelAsync(plantRepository);
            viewModel.PlantList.SelectedPlant = viewModel.PlantList.Plants[0];

            var raisedProperties = new List<string>();
            viewModel.PlantList.PropertyChanged += (_, e) => raisedProperties.Add(e.PropertyName!);

            // When: the selected plant is renamed
            await viewModel.PlantList.RenamePlantAsync(viewModel.PlantList.Plants[0], "Basil");

            // Then: SelectedPlant was announced, so the dashboard header - bound to
            // SelectedPlant.Name - re-reads the name instead of keeping the old one
            Assert.Contains(nameof(PlantListViewModel.SelectedPlant), raisedProperties);
        }

        [Fact]
        public async Task RenamePlantAsync_GivenSearchIsActive_AppliesTheSearchToTheNewName()
        {
            // Given: an initialized MainViewModel whose sidebar is filtered down to the aloe vera
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(AloeVera(), PlantNamed("Basil"));

            var viewModel = await CreateInitializedViewModelAsync(plantRepository);
            viewModel.PlantList.SearchText = "aloe";

            // When: the aloe vera gets a name the search does not match
            await viewModel.PlantList.RenamePlantAsync(viewModel.PlantList.Plants[0], "Snake Plant");

            // Then: the sidebar was refreshed and no longer lists the plant - without
            // the refresh the entry would stay and keep showing the old name
            Assert.Empty(VisiblePlantNames(viewModel));
        }

        [Fact]
        public async Task RenamePlantAsync_GivenRepositoryThrows_PropagatesExceptionAndKeepsOldName()
        {
            // Given: a plant and a repository configured to fail on rename
            var plant = AloeVera();
            var plantRepository = new FakePlantRepository { ShouldThrowOnRename = true };
            plantRepository.SeedPlants(plant);

            var viewModel = await CreateInitializedViewModelAsync(plantRepository);

            var selectedPlant = viewModel.PlantList.Plants[0];

            // When: renaming is attempted
            var exception = await Record.ExceptionAsync(
                () => viewModel.PlantList.RenamePlantAsync(selectedPlant, "Basil"));

            // Then: the exception is propagated to the caller (MainWindow catches it
            // and shows an error dialog), and the in-memory name stays untouched
            Assert.IsType<InvalidOperationException>(exception);
            Assert.Equal("Aloe Vera", selectedPlant.Name);
        }

        // -- Announcement Tests --
        // What a screen reader says after an action: the result is otherwise only visible.

        [Fact]
        public async Task AddPlantAsync_GivenNewPlant_AnnouncesItsName()
        {
            // Given: an empty plant list, and a listener for announcements
            var viewModel = await CreateInitializedViewModelAsync(new FakePlantRepository());
            var announcements = AnnouncementsOf(viewModel);

            // When: a plant is added
            await viewModel.PlantList.AddPlantAsync(new Plant { Name = "Basil" });

            // Then: the announcement names the new plant
            Assert.Equal(new[] { "Basil added" }, announcements);
        }

        [Fact]
        public async Task RenamePlantAsync_GivenNewName_AnnouncesTheNewName()
        {
            // Given: one plant, and a listener for announcements
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(AloeVera());
            var viewModel = await CreateInitializedViewModelAsync(plantRepository);
            var announcements = AnnouncementsOf(viewModel);

            // When: the plant is renamed
            await viewModel.PlantList.RenamePlantAsync(viewModel.PlantList.Plants[0], "Aloe Arborescens");

            // Then: the announcement names the plant by its new name
            Assert.Equal(new[] { "Renamed to Aloe Arborescens" }, announcements);
        }

        [Theory]
        [InlineData("LILY", "2 plants found")]   // in capitals: the search ignores case
        [InlineData("bas", "1 plant found")]
        [InlineData("xyz", "No plants found")]
        public async Task SearchText_GivenSearchTerm_AnnouncesHowManyPlantsMatch(string searchText, string expected)
        {
            // Given: two lilies and a basil
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(PlantNamed("Peace Lily"), PlantNamed("Basil"), PlantNamed("Calla Lily"));
            var viewModel = await CreateInitializedViewModelAsync(plantRepository);
            var announcements = AnnouncementsOf(viewModel);

            // When: the term is typed into the search box
            viewModel.PlantList.SearchText = searchText;

            // Then: the number of results is announced, since the list changes without the focus moving
            Assert.Equal(new[] { expected }, announcements);
        }

        [Fact]
        public async Task SearchText_GivenSearchIsEmptied_AnnouncesNothing()
        {
            // Given: an active search, and a listener that starts afterwards
            var plantRepository = new FakePlantRepository();
            plantRepository.SeedPlants(PlantNamed("Peace Lily"), PlantNamed("Basil"));
            var viewModel = await CreateInitializedViewModelAsync(plantRepository);
            viewModel.PlantList.SearchText = "bas";
            var announcements = AnnouncementsOf(viewModel);

            // When: the search box is emptied
            viewModel.PlantList.SearchText = string.Empty;

            // Then: the full list is back and needs no announcement
            Assert.Empty(announcements);
        }
    }
}
