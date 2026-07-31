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
    }
}
