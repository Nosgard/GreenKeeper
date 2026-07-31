using GreenKeeper.Tests.Fakes;
using GreenKeeper.ViewModels;
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
    }
}
