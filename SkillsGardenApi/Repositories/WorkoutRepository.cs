using Microsoft.EntityFrameworkCore;
using SkillsGardenApi.Models;
using SkillsGardenApi.Repositories.Context;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkillsGardenApi.Repositories
{
    public class WorkoutRepository : IDatabaseRepository<Workout>
    {
        private readonly DatabaseContext ctx;

        public WorkoutRepository(DatabaseContext ctx)
        {
            this.ctx = ctx;
        }

        /**
         * Create
         */
        public async Task<Workout> CreateAsync(Workout workout)
        {
            ctx.Workouts.Add(workout);
            await ctx.SaveChangesAsync();
            return workout;
        }

        /**
         * Delete
         */
        public async Task<bool> DeleteAsync(int workoutId)
        {
            Workout workout = await ReadAsync(workoutId);
            if (workout == null)
                return false;
            ctx.Workouts.Remove(workout);
            await ctx.SaveChangesAsync();
            return true;
        }

        /**
         * List
         */
        public async Task<List<Workout>> ListAsync()
        {
            return await ctx.Workouts
                .Include(l => l.Exercises)
                .ToListAsync();
        }

        /**
         * Read
         */
        public async Task<Workout> ReadAsync(int workoutId)
        {
            return await ctx.Workouts
                .Include(e => e.Exercises)
                .Where(l => l.Id == workoutId)
                .FirstOrDefaultAsync();
        }

        /**
         * Update
         */
        public async Task<Workout> UpdateAsync(Workout workout)
        {
            var workoutToBeUpdated = await ReadAsync(workout.Id);
            if (workoutToBeUpdated == null || workout == null)
            {
                return null;
            }

            // update only if set
            if (workout.Name != null) workoutToBeUpdated.Name = workout.Name;
            if (workout.Type != null) workoutToBeUpdated.Type = workout.Type;
            if (workout.Exercises != null) workoutToBeUpdated.Exercises = workout.Exercises;

            await ctx.SaveChangesAsync();
            return workoutToBeUpdated;
        }

        /**
         * Exists
         */
        public async Task<bool> WorkoutExists(int id)
        {
            return await ctx.Workouts.FirstOrDefaultAsync(x => x.Id == id) != null;
        }
    }
}
