using Microsoft.EntityFrameworkCore;
using SkillsGardenApi.Models;
using SkillsGardenApi.Repositories.Context;
using SkillsGardenDTO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkillsGardenApi.Repositories
{
    public class ExerciseRepository : IDatabaseRepository<Exercise>
    {
        private readonly DatabaseContext ctx;

        public ExerciseRepository(DatabaseContext ctx)
        {
            this.ctx = ctx;
        }

        /**
         * Create
         */
        public async Task<Exercise> CreateAsync(Exercise exercise)
        {
            if (await ExerciseExists(exercise.Id))
                return null;
            ctx.Exercises.Add(exercise);
            await ctx.SaveChangesAsync();
            return exercise;
        }

        /*
         * Delete
         */
        public async Task<bool> DeleteAsync(int id)
        {
            Exercise exercise = await ReadAsync(id);
            if (exercise == null)
            {
                return false;
            }
            ctx.Exercises.Remove(exercise);
            ctx.ComponentExercises.RemoveRange(ctx.ComponentExercises.Where(f => f.ExerciseId == id));
            ctx.WorkoutExercises.RemoveRange(ctx.WorkoutExercises.Where(r => r.ExerciseId == id));
            await ctx.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteRequirementsAsync(int id)
        {
            Exercise exercise = await ReadAsync(id);
            if (exercise == null)
            {
                return false;
            }
            ctx.ExerciseRequirements.RemoveRange(ctx.ExerciseRequirements.Where(r => r.Exercise.Id == id));
            await ctx.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteStepsAsync(int id)
        {
            Exercise exercise = await ReadAsync(id);
            if (exercise == null)
            {
                return false;
            }
            ctx.ExerciseSteps.RemoveRange(ctx.ExerciseSteps.Where(s => s.Exercise.Id == id));
            await ctx.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteFormsAsync(int id)
        {
            Exercise exercise = await ReadAsync(id);
            if (exercise == null)
            {
                return false;
            }
            ctx.ExerciseForms.RemoveRange(ctx.ExerciseForms.Where(f => f.Exercise.Id == id));
            await ctx.SaveChangesAsync();
            return true;
        }

        /**
         * List
         */
        public async Task<List<Exercise>> ListAsync()
        {
            return await ctx.Exercises
                .Include(e => e.ExerciseRequirements)
                .Include(e => e.ExerciseSteps)
                .Include(e => e.ExerciseForms)
                .ToListAsync();
        }

        public async Task<List<Exercise>> ListAsyncByMovementForm(List<MovementForm> movementForms)
        {
            return await ctx.Exercises
                .Include(e => e.ExerciseRequirements)
                .Include(e => e.ExerciseSteps)
                .Include(e => e.ExerciseForms)
                .Where(e => e.ExerciseForms.Where(f => movementForms.Contains(f.MovementForm)).Any())
                .ToListAsync();
        }

        /**
         * Read
         */
        public async Task<Exercise> ReadAsync(int id)
        {
            return await ctx.Exercises
                .Include(e => e.ExerciseRequirements)
                .Include(e => e.ExerciseSteps)
                .Include(e => e.ExerciseForms)
                .Where(e => e.Id == id).FirstOrDefaultAsync();
        }

        /**
         * Update
         */
        public async Task<Exercise> UpdateAsync(Exercise exercise)
        {
            Exercise exerciseToBeUpdated = await ReadAsync(exercise.Id);
            if (exerciseToBeUpdated == null || exercise == null)
            {
                return null;
            }

            // update only if set
            if (exercise.Name != null) exerciseToBeUpdated.Name = exercise.Name;
            if (exercise.Media != null) exerciseToBeUpdated.Media = exercise.Media;
            if (exercise.ExerciseRequirements != null) exerciseToBeUpdated.ExerciseRequirements = exercise.ExerciseRequirements;
            if (exercise.ExerciseSteps != null) exerciseToBeUpdated.ExerciseSteps = exercise.ExerciseSteps;
            if (exercise.ExerciseForms != null) exerciseToBeUpdated.ExerciseForms = exercise.ExerciseForms;

            await ctx.SaveChangesAsync();
            return exerciseToBeUpdated;
        }

        /**
         * Exists
         */
        public async Task<bool> ExerciseExists(int id)
        {
            return await ctx.Exercises.FirstOrDefaultAsync(e => e.Id == id) != null;
        }

        public async Task<bool> ExerciseRequirementExists(int id)
        {
            return await ctx.ExerciseRequirements.FirstOrDefaultAsync(e => e.Id == id) != null;
        }
    }
}
