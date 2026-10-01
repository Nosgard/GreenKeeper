using GreenKeeper.Database;
using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

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
        private readonly PlantRepository _repository;

        public PlantRepositoryTests()
        {
            _connection.Open();
            var contextFactory = new InMemoryDbContextFactory(_connection);

            using var context = contextFactory.CreateDbContext();
            context.Database.Migrate();

            _repository = new PlantRepository(contextFactory);
        }

        public void Dispose()
        {
            _connection.Dispose();
        }

        // -- Add Or Replace Care Schedule Tests --

        [Fact]
        public async Task AddOrReplaceCareScheduleAsync_GivenExistingCareType_ReplacesTheStoredSchedule()
        {
            // Given: a stored plant that is fertilized every 30 days
            var plant = await _repository.AddPlantAsync(CreatePlantFertilizedEvery(30));

            // When: the schedule is replaced by one with a 14-day interval
            await _repository.AddOrReplaceCareScheduleAsync(plant.Id, CreateFertilizingSchedule(14));

            // Then: only the new schedule is stored
            var storedSchedule = Assert.Single((await _repository.GetPlantsAsync()).Single().CareSchedules);
            Assert.Equal(14, storedSchedule.IntervalAmount);
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
            var plant = await _repository.AddPlantAsync(CreatePlantFertilizedEvery(30));
            FailInsertsInto(nameof(GreenKeeperDbContext.CareSchedules));

            // When: the schedule is replaced, the failed insert reaches the caller
            await Assert.ThrowsAsync<DbUpdateException>(
                () => _repository.AddOrReplaceCareScheduleAsync(plant.Id, CreateFertilizingSchedule(14)));

            // Then: the old schedule is still stored
            var storedSchedule = Assert.Single((await _repository.GetPlantsAsync()).Single().CareSchedules);
            Assert.Equal(30, storedSchedule.IntervalAmount);
        }

        // -- Add Or Replace Sunlight Requirement Tests --

        [Fact]
        public async Task AddOrReplaceSunlightRequirementAsync_GivenExistingRequirement_ReplacesTheStoredRequirement()
        {
            // Given: a stored plant that needs 6 hours of sunlight per day
            var plant = await _repository.AddPlantAsync(CreatePlantNeedingDailySunlight(6));

            // When: the requirement is replaced by 8 hours per day
            await _repository.AddOrReplaceSunlightRequirementAsync(plant.Id, CreateDailySunlightRequirement(8));

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
            var plant = await _repository.AddPlantAsync(CreatePlantNeedingDailySunlight(6));
            FailInsertsInto(nameof(GreenKeeperDbContext.SunlightRequirements));

            // When: the requirement is replaced, the failed insert reaches the caller
            await Assert.ThrowsAsync<DbUpdateException>(
                () => _repository.AddOrReplaceSunlightRequirementAsync(plant.Id, CreateDailySunlightRequirement(8)));

            // Then: the old requirement is still stored
            var storedRequirement = (await _repository.GetPlantsAsync()).Single().SunlightRequirement;
            Assert.NotNull(storedRequirement);
            Assert.Equal(6, storedRequirement.Hours);
        }

        // -- Helpers --

        private static Plant CreatePlantFertilizedEvery(int days)
        {
            var plant = new Plant { Name = "Aloe Vera" };
            plant.CareSchedules.Add(CreateFertilizingSchedule(days));
            return plant;
        }

        private static CareSchedule CreateFertilizingSchedule(int days)
        {
            return new CareSchedule { Care = CareType.Fertilizing, IntervalAmount = days, IntervalUnit = TimeUnit.Days };
        }

        private static Plant CreatePlantNeedingDailySunlight(int hours)
        {
            return new Plant { Name = "Aloe Vera", SunlightRequirement = CreateDailySunlightRequirement(hours) };
        }

        private static SunlightRequirement CreateDailySunlightRequirement(int hours)
        {
            return new SunlightRequirement { Hours = hours, Period = SunlightPeriod.Day };
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
