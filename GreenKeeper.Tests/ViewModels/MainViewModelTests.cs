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
    }
}
