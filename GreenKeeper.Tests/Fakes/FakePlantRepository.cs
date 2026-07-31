using GreenKeeper.Models;
using GreenKeeper.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.Tests.Fakes
{
    /// <summary>
    /// In-memory fake for IPlantRepository. Behaves like a tiny, self-contained
    /// database: every method operates on the same internal list, so tests can
    /// verify realistic sequences (e.g. "after AddPlantAsync, GetPlantsAsync
    /// returns the new plant") without a real SQLite database or DbContext.
    /// 
    /// Tests can pre-populate the repository via SeedPlants(...) before
    /// creating a MainViewModel, to set up a "Given" state
    /// </summary>
    public class FakePlantRepository : IPlantRepository
    {
        private readonly List<Plant> _plants = new();
        private int _nextId = 1;

        public bool ShouldThrowOnAdd { get; set; }
        public bool ShouldThrowOnDelete { get; set; }

        public void SeedPlants(params Plant[] plants)
        {
            foreach (var plant in plants)
            {
                if (plant.Id == 0)
                {
                    plant.Id = _nextId++;
                }
                _plants.Add(plant);
            }
        }

        public Task<List<Plant>> GetPlantsAsync()
        {
            return Task.FromResult(_plants.ToList());
        }

        public Task<Plant> AddPlantAsync(Plant plant)
        {
            if (ShouldThrowOnAdd)
            {
                throw new InvalidOperationException("Simulated database failure");
            }

            plant.Id = _nextId++;

            foreach (var schedule in plant.CareSchedules)
            {
                schedule.Id = _nextId++;
                schedule.PlantId = plant.Id;
            }

            if (plant.SunlightRequirement != null)
            {
                plant.SunlightRequirement.Id = _nextId++;
                plant.SunlightRequirement.PlantId = plant.Id;
            }

            _plants.Add(plant);
            return Task.FromResult(plant);
        }

        public Task CompleteCareScheduleAsync(int careScheduleId, DateTime nextDueAt, DateTime lastCaredAt)
        {
            var schedule = _plants
                .SelectMany(p => p.CareSchedules)
                .FirstOrDefault(s => s.Id == careScheduleId);

            if (schedule == null)
            {
                throw new InvalidOperationException($"Care-Schedule with Id {careScheduleId} was not found");
            }

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

            var plant = _plants.FirstOrDefault(p => p.Id == plantId);

            if (plant == null)
            {
                throw new InvalidOperationException($"Plant with Id {plantId} was not found");
            }

            _plants.Remove(plant);
            return Task.CompletedTask;
        }

        public Task<CareSchedule> AddOrReplaceCareScheduleAsync(int plantId, CareSchedule careSchedule)
        {
            var plant = _plants.FirstOrDefault(p => p.Id == plantId)
                ?? throw new InvalidOperationException($"Plant with Id {plantId} was not found");

            var existing = plant.CareSchedules.FirstOrDefault(s => s.Care == careSchedule.Care);
            if (existing != null)
            {
                plant.CareSchedules.Remove(existing);
            }

            careSchedule.Id = _nextId++;
            careSchedule.PlantId = plantId;
            plant.CareSchedules.Add(careSchedule);

            return Task.FromResult(careSchedule);
        }

        public Task<SunlightRequirement> AddOrReplaceSunlightRequirementAsync(int plantId, SunlightRequirement sunlightRequirement)
        {
            var plant = _plants.FirstOrDefault(p => p.Id == plantId)
            ?? throw new InvalidOperationException($"Plant with Id {plantId} was not found.");

            sunlightRequirement.Id = _nextId++;
            sunlightRequirement.PlantId = plantId;
            plant.SunlightRequirement = sunlightRequirement;

            return Task.FromResult(sunlightRequirement);
        }

        public Task RemoveCareScheduleAsync(int careScheduleId)
        {
            var plant = _plants.FirstOrDefault(p => p.CareSchedules.Any(s => s.Id == careScheduleId))
            ?? throw new InvalidOperationException($"CareSchedule with Id {careScheduleId} was not found.");

            var schedule = plant.CareSchedules.First(s => s.Id == careScheduleId);
            plant.CareSchedules.Remove(schedule);

            return Task.CompletedTask;
        }

        public Task RemoveSunlightRequirementAsync(int sunlightRequirementId)
        {
            var plant = _plants.FirstOrDefault(p => p.SunlightRequirement?.Id == sunlightRequirementId)
            ?? throw new InvalidOperationException($"SunlightRequirement with Id {sunlightRequirementId} was not found.");

            plant.SunlightRequirement = null;

            return Task.CompletedTask;
        }

        public Task UpdatePlantNotesAsync(int plantId, string notes)
        {
            var plant = _plants.FirstOrDefault(p => p.Id == plantId)
                ?? throw new InvalidOperationException($"Plant with Id {plantId} was not found");

            plant.Notes = notes;
            return Task.CompletedTask;
        }
    }
}
