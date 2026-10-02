using GreenKeeper.Database;
using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using static GreenKeeper.Tests.TestPlants;

namespace GreenKeeper.Tests.Repositories
{
    /// <summary>
    /// Runs PlantRepository against a real SQLite database that only lives in memory,
    /// so that transactions and constraints behave exactly like in the app.
    /// xUnit creates a new instance for every test, so each test starts with an empty database.
    /// </summary>
    public class PlantRepositoryTests : IDisposable
    {
        // The in-memory database only exists as long as this connection stays open.
        private readonly SqliteConnection _connection = new("Data Source=:memory:");
        private readonly InMemoryDbContextFactory _contextFactory;
        private readonly PlantRepository _repository;

        public PlantRepositoryTests()
        {
            _connection.Open();
            _contextFactory = new InMemoryDbContextFactory(_connection);

            using var context = _contextFactory.CreateDbContext();
            context.Database.Migrate();

            _repository = new PlantRepository(_contextFactory);
        }

        public void Dispose()
        {
            _connection.Dispose();
        }

        // -- Complete Care Schedule Tests --

        [Fact]
        public async Task CompleteCareScheduleAsync_GivenStoredSchedule_StoresTheNewDueDateAndTheDateOfCare()
        {
            // Given: a stored plant that is watered every 7 days, and the moment it is watered
            var plant = await _repository.AddPlantAsync(AloeVera(WateringEveryDays(7)));
            var watering = plant.CareSchedules.Single();
            var caredAt = new DateTime(2025, 5, 1, 10, 30, 0);
            var nextDueAt = new DateTime(2025, 5, 8, 10, 30, 0);

            // When: the watering is completed
            await _repository.CompleteCareScheduleAsync(watering.Id, nextDueAt, caredAt);

            // Then: both dates are stored on the schedule
            var storedSchedule = Assert.Single((await _repository.GetPlantsAsync()).Single().CareSchedules);
            Assert.Equal(nextDueAt, storedSchedule.NextDueAt);
            Assert.Equal(caredAt, storedSchedule.LastCaredAt);
        }

        // -- Delete Plant Tests --

        [Fact]
        public async Task DeletePlantAsync_GivenPlantWithScheduleAndSunlight_RemovesThePlantAndEverythingAttached()
        {
            // Given: a stored plant with a watering schedule and a sunlight requirement
            var plant = await _repository.AddPlantAsync(AloeVeraNeedingDailySunlight(6, WateringEveryDays(7)));

            // When: the plant is deleted
            await _repository.DeletePlantAsync(plant.Id);

            // Then: no row of the plant is left - an orphaned schedule or requirement
            // would never be shown again, but stay in the database for good
            using var context = _contextFactory.CreateDbContext();
            Assert.Empty(context.Plants);
            Assert.Empty(context.CareSchedules);
            Assert.Empty(context.SunlightRequirements);
        }

        // -- Add Or Replace Care Schedule Tests --

        [Fact]
        public async Task AddOrReplaceCareScheduleAsync_GivenNoScheduleOfThatCareType_AddsItNextToTheExistingOne()
        {
            // Given: a stored plant that is only watered so far
            var plant = await _repository.AddPlantAsync(AloeVera(WateringEveryDays(7)));

            // When: a fertilizing schedule is saved for it
            await _repository.AddOrReplaceCareScheduleAsync(plant.Id, FertilizingEveryDays(30));

            // Then: the plant has both schedules
            var storedSchedules = (await _repository.GetPlantsAsync()).Single().CareSchedules;
            Assert.Equal(2, storedSchedules.Count);
            Assert.Contains(storedSchedules, s => s.Care == CareType.Watering);
            Assert.Contains(storedSchedules, s => s.Care == CareType.Fertilizing);
        }

        [Fact]
        public async Task AddOrReplaceCareScheduleAsync_GivenExistingCareType_ReplacesTheStoredSchedule()
        {
            // Given: a stored plant that is fertilized every 30 days
            var plant = await _repository.AddPlantAsync(AloeVera(FertilizingEveryDays(30)));

            // When: the schedule is replaced by one with a 14-day interval
            await _repository.AddOrReplaceCareScheduleAsync(plant.Id, FertilizingEveryDays(14));

            // Then: only the new schedule is stored
            var storedSchedule = Assert.Single((await _repository.GetPlantsAsync()).Single().CareSchedules);
            Assert.Equal(14, storedSchedule.IntervalAmount);
        }

        [Fact]
        public async Task AddOrReplaceCareScheduleAsync_GivenAnotherPlantWithTheSameCareType_LeavesThatPlantUntouched()
        {
            // Given: two stored plants, both fertilized every 30 days
            var aloeVera = await _repository.AddPlantAsync(AloeVera(FertilizingEveryDays(30)));
            var basil = await _repository.AddPlantAsync(PlantNamed("Basil", FertilizingEveryDays(30)));

            // When: the schedule of the basil is replaced by one with a 14-day interval
            await _repository.AddOrReplaceCareScheduleAsync(basil.Id, FertilizingEveryDays(14));

            // Then: the aloe vera still has its own 30-day schedule
            var storedAloeVera = (await _repository.GetPlantsAsync()).Single(p => p.Id == aloeVera.Id);
            var storedSchedule = Assert.Single(storedAloeVera.CareSchedules);
            Assert.Equal(30, storedSchedule.IntervalAmount);
        }

        /// <summary>
        /// Regression test for a lost schedule: the old schedule used to be deleted and
        /// saved before the new one was inserted, so a failing insert left the plant
        /// without any schedule of that care type.
        /// </summary>
        [Fact]
        public async Task AddOrReplaceCareScheduleAsync_GivenTheInsertFails_KeepsTheExistingSchedule()
        {
            // Given: a stored plant that is fertilized every 30 days,
            // and a database that rejects every new care schedule
            var plant = await _repository.AddPlantAsync(AloeVera(FertilizingEveryDays(30)));
            FailInsertsInto(nameof(GreenKeeperDbContext.CareSchedules));

            // When: the schedule is replaced
            var exception = await Record.ExceptionAsync(
                () => _repository.AddOrReplaceCareScheduleAsync(plant.Id, FertilizingEveryDays(14)));

            // Then: the failed insert reaches the caller, and the old schedule is still stored
            Assert.IsType<DbUpdateException>(exception);
            var storedSchedule = Assert.Single((await _repository.GetPlantsAsync()).Single().CareSchedules);
            Assert.Equal(30, storedSchedule.IntervalAmount);
        }

        // -- Add Or Replace Sunlight Requirement Tests --

        [Fact]
        public async Task AddOrReplaceSunlightRequirementAsync_GivenOnlyAnotherPlantHasARequirement_AddsOneForTheGivenPlantOnly()
        {
            // Given: a stored aloe vera that needs 6 hours of sunlight per day,
            // and a stored basil without a requirement
            var aloeVera = await _repository.AddPlantAsync(AloeVeraNeedingDailySunlight(6));
            var basil = await _repository.AddPlantAsync(PlantNamed("Basil"));

            // When: a requirement of 8 hours per day is saved for the basil
            await _repository.AddOrReplaceSunlightRequirementAsync(basil.Id, DailySunlight(8));

            // Then: each plant has its own requirement
            var storedPlants = await _repository.GetPlantsAsync();
            Assert.Equal(8, storedPlants.Single(p => p.Id == basil.Id).SunlightRequirement?.Hours);
            Assert.Equal(6, storedPlants.Single(p => p.Id == aloeVera.Id).SunlightRequirement?.Hours);
        }

        [Fact]
        public async Task AddOrReplaceSunlightRequirementAsync_GivenExistingRequirement_ReplacesTheStoredRequirement()
        {
            // Given: a stored plant that needs 6 hours of sunlight per day
            var plant = await _repository.AddPlantAsync(AloeVeraNeedingDailySunlight(6));

            // When: the requirement is replaced by 8 hours per day
            await _repository.AddOrReplaceSunlightRequirementAsync(plant.Id, DailySunlight(8));

            // Then: the new requirement is stored
            var storedRequirement = (await _repository.GetPlantsAsync()).Single().SunlightRequirement;
            Assert.NotNull(storedRequirement);
            Assert.Equal(8, storedRequirement.Hours);
        }

        /// <summary>
        /// Regression test for a lost requirement: the old requirement used to be deleted
        /// and saved before the new one was inserted, so a failing insert left the plant
        /// without any sunlight requirement.
        /// </summary>
        [Fact]
        public async Task AddOrReplaceSunlightRequirementAsync_GivenTheInsertFails_KeepsTheExistingRequirement()
        {
            // Given: a stored plant that needs 6 hours of sunlight per day,
            // and a database that rejects every new sunlight requirement
            var plant = await _repository.AddPlantAsync(AloeVeraNeedingDailySunlight(6));
            FailInsertsInto(nameof(GreenKeeperDbContext.SunlightRequirements));

            // When: the requirement is replaced
            var exception = await Record.ExceptionAsync(
                () => _repository.AddOrReplaceSunlightRequirementAsync(plant.Id, DailySunlight(8)));

            // Then: the failed insert reaches the caller, and the old requirement is still stored
            Assert.IsType<DbUpdateException>(exception);
            var storedRequirement = (await _repository.GetPlantsAsync()).Single().SunlightRequirement;
            Assert.NotNull(storedRequirement);
            Assert.Equal(6, storedRequirement.Hours);
        }

        // -- Remove Care Schedule Tests --

        [Fact]
        public async Task RemoveCareScheduleAsync_GivenPlantWithTwoSchedules_RemovesOnlyTheGivenOne()
        {
            // Given: a stored plant that is watered every 7 days and fertilized every 30 days
            var plant = await _repository.AddPlantAsync(AloeVera(WateringEveryDays(7), FertilizingEveryDays(30)));
            var fertilizing = plant.CareSchedules.Single(s => s.Care == CareType.Fertilizing);

            // When: the fertilizing schedule is removed
            await _repository.RemoveCareScheduleAsync(fertilizing.Id);

            // Then: only the watering schedule is still stored
            var storedSchedule = Assert.Single((await _repository.GetPlantsAsync()).Single().CareSchedules);
            Assert.Equal(CareType.Watering, storedSchedule.Care);
        }

        // -- Remove Sunlight Requirement Tests --

        [Fact]
        public async Task RemoveSunlightRequirementAsync_GivenStoredRequirement_RemovesItAndKeepsThePlant()
        {
            // Given: a stored plant that needs 6 hours of sunlight per day
            var plant = await _repository.AddPlantAsync(AloeVeraNeedingDailySunlight(6));
            var requirement = plant.SunlightRequirement!;

            // When: the requirement is removed
            await _repository.RemoveSunlightRequirementAsync(requirement.Id);

            // Then: the plant is still stored, but without a requirement
            var storedPlant = Assert.Single(await _repository.GetPlantsAsync());
            Assert.Null(storedPlant.SunlightRequirement);
        }

        // -- Plant Notes Tests --

        [Fact]
        public async Task UpdatePlantNotesAsync_GivenStoredPlant_StoresTheNotes()
        {
            // Given: a stored plant without notes
            var plant = await _repository.AddPlantAsync(AloeVera());

            // When: notes are saved for it
            await _repository.UpdatePlantNotesAsync(plant.Id, "Loves indirect sunlight.");

            // Then: the notes are stored on the plant
            var storedPlant = Assert.Single(await _repository.GetPlantsAsync());
            Assert.Equal("Loves indirect sunlight.", storedPlant.Notes);
        }

        // -- Rename Plant Tests --

        [Fact]
        public async Task RenamePlantAsync_GivenStoredPlant_StoresTheNewName()
        {
            // Given: a stored plant named "Aloe Vera"
            var plant = await _repository.AddPlantAsync(AloeVera());

            // When: it is renamed to "Basil"
            await _repository.RenamePlantAsync(plant.Id, "Basil");

            // Then: the new name is stored
            var storedPlant = Assert.Single(await _repository.GetPlantsAsync());
            Assert.Equal("Basil", storedPlant.Name);
        }

        // -- Missing Row Tests --

        /// <summary>
        /// A row can vanish while a window still shows it. The message of the exception
        /// ends up in the error dialog, so it has to say what was not found.
        /// </summary>
        [Theory]
        [InlineData("CompleteCareSchedule", "Care schedule with Id 99 was not found.")]
        [InlineData("RemoveCareSchedule", "Care schedule with Id 99 was not found.")]
        [InlineData("RemoveSunlightRequirement", "Sunlight requirement with Id 99 was not found.")]
        [InlineData("DeletePlant", "Plant with Id 99 was not found.")]
        [InlineData("UpdatePlantNotes", "Plant with Id 99 was not found.")]
        [InlineData("RenamePlant", "Plant with Id 99 was not found.")]
        public async Task Operation_GivenIdOfAMissingRow_ThrowsAnExceptionNamingTheRow(string operation, string expectedMessage)
        {
            // Given: an empty database, so no row carries the Id 99
            const int missingId = 99;

            // When: the operation is run for that Id
            var exception = await Record.ExceptionAsync(() => RunOperation(operation, missingId));

            // Then: the exception says which row is missing
            var notFound = Assert.IsType<InvalidOperationException>(exception);
            Assert.Equal(expectedMessage, notFound.Message);
        }

        // -- Helpers --

        // Maps the name from [InlineData] to the repository call - attributes can only
        // carry constants, no delegates. The values next to the Id are never stored.
        private Task RunOperation(string operation, int id) => operation switch
        {
            "CompleteCareSchedule" => _repository.CompleteCareScheduleAsync(id, DateTime.MinValue, DateTime.MinValue),
            "RemoveCareSchedule" => _repository.RemoveCareScheduleAsync(id),
            "RemoveSunlightRequirement" => _repository.RemoveSunlightRequirementAsync(id),
            "DeletePlant" => _repository.DeletePlantAsync(id),
            "UpdatePlantNotes" => _repository.UpdatePlantNotesAsync(id, "Any notes"),
            "RenamePlant" => _repository.RenamePlantAsync(id, "Any name"),
            _ => throw new ArgumentOutOfRangeException(nameof(operation))
        };

        private static Plant AloeVeraNeedingDailySunlight(int hours, params CareSchedule[] schedules)
        {
            var plant = AloeVera(schedules);
            plant.SunlightRequirement = DailySunlight(hours);
            return plant;
        }

        // Simulates a database failure: SQLite aborts every INSERT into the given table.
        private void FailInsertsInto(string tableName)
        {
            using var command = _connection.CreateCommand();
            command.CommandText =
                $"CREATE TRIGGER Fail{tableName}Insert BEFORE INSERT ON {tableName} " +
                "BEGIN SELECT RAISE(ABORT, 'Simulated database failure'); END";
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Hands out contexts that all share the one open in-memory connection,
        /// just like the app's factory hands out contexts for the one database file.
        /// </summary>
        private sealed class InMemoryDbContextFactory : IDbContextFactory<GreenKeeperDbContext>
        {
            private readonly DbContextOptions<GreenKeeperDbContext> _options;

            public InMemoryDbContextFactory(SqliteConnection connection)
            {
                _options = new DbContextOptionsBuilder<GreenKeeperDbContext>()
                    .UseSqlite(connection)
                    .Options;
            }

            public GreenKeeperDbContext CreateDbContext()
            {
                return new GreenKeeperDbContext(_options);
            }
        }
    }
}
