using GreenKeeper.Models;
using GreenKeeper.Repositories;

namespace GreenKeeper.Tests.Fakes
{
    /// <summary>
    /// In-memory fake for IPlantRepository. Behaves like a tiny, self-contained
    /// database: every method operates on the same internal list, so tests can
    /// verify realistic sequences (e.g. "after AddPlantAsync, GetPlantsAsync
    /// returns the new plant") without a real SQLite database or DbContext.
    ///
    /// Like the real repository, it never shares the objects it stores: what is
    /// handed in is copied before it is stored, and what is handed out is a copy
    /// of what is stored. A change to a stored plant therefore does not show up in
    /// the objects a ViewModel holds, and the other way round - so a test can tell
    /// whether the ViewModel went through the repository, updated its own objects,
    /// or did both.
    ///
    /// Tests can pre-populate the repository via SeedPlants(...) before
    /// creating a MainViewModel, to set up a "Given" state.
    /// </summary>
    public class FakePlantRepository : IPlantRepository
    {
        private readonly List<Plant> _plants = new();
        private int _nextId = 1;

        public bool ShouldThrowOnAdd { get; set; }
        public bool ShouldThrowOnDelete { get; set; }
        public int CompleteCareScheduleAsyncCallCount { get; private set; }
        public int AddOrReplaceCareScheduleAsyncCallCount { get; private set; }
        public int AddOrReplaceSunlightRequirementAsyncCallCount { get; private set; }
        public bool ShouldThrowOnCompleteCareSchedule { get; set; }
        public bool ShouldThrowOnRemoveCareSchedule { get; set; }
        public bool ShouldThrowOnRemoveSunlightRequirement { get; set; }
        public bool ShouldThrowOnAddOrReplaceCareSchedule { get; set; }
        public bool ShouldThrowOnAddOrReplaceSunlightRequirement { get; set; }
        public bool ShouldThrowOnUpdateNotes { get; set; }
        public bool ShouldThrowOnRename { get; set; }

        public void SeedPlants(params Plant[] plants)
        {
            foreach (var plant in plants)
            {
                AssignIds(plant);
                _plants.Add(Copy(plant));
            }
        }

        // Gives the plant and everything attached to it an Id, like the database
        // would on insert. Ids that are already set stay as they are.
        private void AssignIds(Plant plant)
        {
            if (plant.Id == 0)
            {
                plant.Id = _nextId++;
            }

            foreach (var schedule in plant.CareSchedules)
            {
                if (schedule.Id == 0)
                {
                    schedule.Id = _nextId++;
                }
                schedule.PlantId = plant.Id;
            }

            if (plant.SunlightRequirement != null)
            {
                if (plant.SunlightRequirement.Id == 0)
                {
                    plant.SunlightRequirement.Id = _nextId++;
                }
                plant.SunlightRequirement.PlantId = plant.Id;
            }
        }

        public Task<List<Plant>> GetPlantsAsync()
        {
            return Task.FromResult(_plants.Select(Copy).ToList());
        }

        public Task<Plant> AddPlantAsync(Plant plant)
        {
            if (ShouldThrowOnAdd)
            {
                throw new InvalidOperationException("Simulated database failure");
            }

            AssignIds(plant);
            _plants.Add(Copy(plant));
            return Task.FromResult(plant);
        }

        public Task CompleteCareScheduleAsync(int careScheduleId, DateTime nextDueAt, DateTime lastCaredAt)
        {
            CompleteCareScheduleAsyncCallCount++;

            if (ShouldThrowOnCompleteCareSchedule)
            {
                throw new InvalidOperationException("Simulated database failure");
            }

            var schedule = _plants
                .SelectMany(p => p.CareSchedules)
                .FirstOrDefault(s => s.Id == careScheduleId)
                ?? throw new InvalidOperationException($"Care schedule with Id {careScheduleId} was not found.");

            schedule.NextDueAt = nextDueAt;
            schedule.LastCaredAt = lastCaredAt;
            return Task.CompletedTask;
        }

        public Task DeletePlantAsync(int plantId)
        {
            if (ShouldThrowOnDelete)
            {
                throw new InvalidOperationException("Simulated database failure");
            }

            var plant = _plants.FirstOrDefault(p => p.Id == plantId)
                ?? throw new InvalidOperationException($"Plant with Id {plantId} was not found.");

            _plants.Remove(plant);
            return Task.CompletedTask;
        }

        public Task<CareSchedule> AddOrReplaceCareScheduleAsync(int plantId, CareSchedule careSchedule)
        {
            AddOrReplaceCareScheduleAsyncCallCount++;

            if (ShouldThrowOnAddOrReplaceCareSchedule)
            {
                throw new InvalidOperationException("Simulated database failure");
            }

            var plant = _plants.FirstOrDefault(p => p.Id == plantId)
                ?? throw new InvalidOperationException($"Plant with Id {plantId} was not found.");

            var existing = plant.CareSchedules.FirstOrDefault(s => s.Care == careSchedule.Care);
            if (existing != null)
            {
                plant.CareSchedules.Remove(existing);
            }

            careSchedule.Id = _nextId++;
            careSchedule.PlantId = plantId;
            plant.CareSchedules.Add(Copy(careSchedule));

            return Task.FromResult(careSchedule);
        }

        public Task<SunlightRequirement> AddOrReplaceSunlightRequirementAsync(int plantId, SunlightRequirement sunlightRequirement)
        {
            AddOrReplaceSunlightRequirementAsyncCallCount++;

            if (ShouldThrowOnAddOrReplaceSunlightRequirement)
            {
                throw new InvalidOperationException("Simulated database failure");
            }

            var plant = _plants.FirstOrDefault(p => p.Id == plantId)
                ?? throw new InvalidOperationException($"Plant with Id {plantId} was not found.");

            sunlightRequirement.Id = _nextId++;
            sunlightRequirement.PlantId = plantId;
            plant.SunlightRequirement = Copy(sunlightRequirement);

            return Task.FromResult(sunlightRequirement);
        }

        public Task RemoveCareScheduleAsync(int careScheduleId)
        {
            if (ShouldThrowOnRemoveCareSchedule)
            {
                throw new InvalidOperationException("Simulated database failure");
            }

            var plant = _plants.FirstOrDefault(p => p.CareSchedules.Any(s => s.Id == careScheduleId))
                ?? throw new InvalidOperationException($"Care schedule with Id {careScheduleId} was not found.");

            var schedule = plant.CareSchedules.First(s => s.Id == careScheduleId);
            plant.CareSchedules.Remove(schedule);

            return Task.CompletedTask;
        }

        public Task RemoveSunlightRequirementAsync(int sunlightRequirementId)
        {
            if (ShouldThrowOnRemoveSunlightRequirement)
            {
                throw new InvalidOperationException("Simulated database failure");
            }

            var plant = _plants.FirstOrDefault(p => p.SunlightRequirement?.Id == sunlightRequirementId)
                ?? throw new InvalidOperationException($"Sunlight requirement with Id {sunlightRequirementId} was not found.");

            plant.SunlightRequirement = null;

            return Task.CompletedTask;
        }

        public Task UpdatePlantNotesAsync(int plantId, string notes)
        {
            if (ShouldThrowOnUpdateNotes)
            {
                throw new InvalidOperationException("Simulated database failure");
            }

            var plant = _plants.FirstOrDefault(p => p.Id == plantId)
                ?? throw new InvalidOperationException($"Plant with Id {plantId} was not found.");

            plant.Notes = notes;
            return Task.CompletedTask;
        }

        public Task RenamePlantAsync(int plantId, string newName)
        {
            if (ShouldThrowOnRename)
            {
                throw new InvalidOperationException("Simulated database failure");
            }

            var plant = _plants.FirstOrDefault(p => p.Id == plantId)
                ?? throw new InvalidOperationException($"Plant with Id {plantId} was not found.");

            plant.Name = newName;
            return Task.CompletedTask;
        }

        // -- Copies --
        // A copy carries every value of its original and is otherwise on its own,
        // like a row the real repository reads from or writes to the database.

        private static Plant Copy(Plant plant)
        {
            var copy = new Plant
            {
                Id = plant.Id,
                Name = plant.Name,
                ImagePath = plant.ImagePath,
                Notes = plant.Notes
            };

            // Like the real repository, a loaded schedule or requirement points back to its plant.
            foreach (var schedule in plant.CareSchedules)
            {
                var scheduleCopy = Copy(schedule);
                scheduleCopy.SelectedPlant = copy;
                copy.CareSchedules.Add(scheduleCopy);
            }

            if (plant.SunlightRequirement != null)
            {
                copy.SunlightRequirement = Copy(plant.SunlightRequirement);
                copy.SunlightRequirement.SelectedPlant = copy;
            }

            return copy;
        }

        private static CareSchedule Copy(CareSchedule schedule)
        {
            return new CareSchedule
            {
                Id = schedule.Id,
                PlantId = schedule.PlantId,
                Care = schedule.Care,
                LastCaredAt = schedule.LastCaredAt,
                NextDueAt = schedule.NextDueAt,
                IntervalUnit = schedule.IntervalUnit,
                IntervalAmount = schedule.IntervalAmount
            };
        }

        private static SunlightRequirement Copy(SunlightRequirement requirement)
        {
            return new SunlightRequirement
            {
                Id = requirement.Id,
                PlantId = requirement.PlantId,
                Hours = requirement.Hours,
                Period = requirement.Period
            };
        }
    }
}
