using Microsoft.AspNetCore.Mvc;
using Microsoft.Spatial;
using SkillsGardenApi.Models;
using SkillsGardenApi.Repositories;
using SkillsGardenApi.Utils;
using SkillsGardenDTO;
using SkillsGardenDTO.Error;
using SkillsGardenDTO.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkillsGardenApi.Services
{
    public class WorkoutService
    {
        private ExerciseRepository exerciseRepository;
        private WorkoutRepository workoutRepository;

        public WorkoutService(IDatabaseRepository<Exercise> exerciseRepository, IDatabaseRepository<Workout> workoutRepository)
        {
            this.exerciseRepository = (ExerciseRepository)exerciseRepository;
            this.workoutRepository = (WorkoutRepository)workoutRepository;
        }

        public async Task<List<WorkoutListResponse>> GetWorkouts()
        {
            // get all workouts
            List<Workout> workouts = await workoutRepository.ListAsync();

            // create response
            List<WorkoutListResponse> response = new List<WorkoutListResponse>();
            foreach (Workout workout in workouts)
            {
                response.Add(new WorkoutListResponse
                {
                    Id = workout.Id,
                    Name = workout.Name,
                    Type = workout.Type,
                    Exercises = workout.Exercises.Count()
                });
            }

            return response;
        }

        public async Task<WorkoutResponse> GetWorkout(int workoutId)
        {
            // get workout
            Workout workout = await workoutRepository.ReadAsync(workoutId);

            // if the workout does not exist
            if (workout == null)
                return null;

            // create response
            WorkoutResponse response = new WorkoutResponse
            {
                Id = workout.Id ,
                Name = workout.Name,
                Type = workout.Type,
                Exercises = new List<ExerciseResponse>()
            };

            foreach (WorkoutExercise workoutExercise in workout.Exercises)
            {
                Exercise exercise = await exerciseRepository.ReadAsync(workoutExercise.ExerciseId);

                // if exercise does not exist
                if (exercise == null)
                    continue;

                response.Exercises.Add(new ExerciseResponse
                {
                    Id = exercise.Id,
                    Name = exercise.Name,
                    Media = exercise.Media,
                    Requirements = exercise.ExerciseRequirements.Select(r => r.Requirement).ToList(),
                    Steps = exercise.ExerciseSteps.Select(s => s.StepDescription).ToList(),
                    Forms = exercise.ExerciseForms.Select(f => f.MovementForm).ToList()
                });
            }

            return response;
        }

        public async Task<int> CreateWorkout(WorkoutBody workoutBody)
        {
            // create workout exercises
            List<WorkoutExercise> exercises = new List<WorkoutExercise>();
            foreach (int exerciseId in workoutBody.Exercises)
            {
                // if the exercise does not exist
                if (!await exerciseRepository.ExerciseExists(exerciseId))
                    continue;

                exercises.Add(new WorkoutExercise
                {
                    ExerciseId = exerciseId
                });
            }

            // create new workout
            Workout newWorkout = new Workout
            {
                Name = workoutBody.Name,
                Type = workoutBody.Type,
                Exercises = exercises
            };

            // save workout to database
            await workoutRepository.CreateAsync(newWorkout);

            return newWorkout.Id;
        }

        public async Task<Workout> UpdateWorkout(WorkoutBody workoutBody, int workoutId)
        {
            Workout oldWorkout = await workoutRepository.ReadAsync(workoutId);

            // create workout
            Workout workout = new Workout
            {
                Id = workoutId,
                Name = workoutBody.Name,
                Type = workoutBody.Type,
            };

            if (workoutBody.Exercises != null)
            {
                // create workout exercises
                List<WorkoutExercise> exercises = new List<WorkoutExercise>();
                foreach (int exerciseId in workoutBody.Exercises)
                {
                    exercises.Add(new WorkoutExercise
                    {
                        ExerciseId = exerciseId
                    });
                }
                workout.Exercises = exercises;
            }

            // save workout to database
            Workout updatedWorkout = await workoutRepository.UpdateAsync(workout);

            return updatedWorkout;
        }

        public async Task<bool> DeleteWorkout(int workoutId)
        {
            // get the workout
            Workout workout = await workoutRepository.ReadAsync(workoutId);

            // if workout was not found
            if (workout == null)
                return false;

            // delete workout from database
            return await workoutRepository.DeleteAsync(workoutId);
        }

        public async Task<bool> Exists(int workoutId)
        {
            return await workoutRepository.WorkoutExists(workoutId);
        }

        public List<MovementForm> ConvertToMovementformList(string movementformsPlain)
        {
            // convert movement forms to list
            List<MovementForm> movementForms = SplitPipeStringUtil.ParseWords<MovementForm>(movementformsPlain);

            // if no movement forms in list
            if (movementForms.Count == 0)
                movementForms = Enum.GetValues(typeof(MovementForm)).Cast<MovementForm>().Select(v => v).ToList();

            return movementForms;
        }

        public async Task<List<ExerciseResponse>> GetExercisesByMovementforms(int amount, List<MovementForm> movementForms)
        {
            // get list of exercises
            List<Exercise> exercisesDb = await exerciseRepository.ListAsyncByMovementForm(movementForms);

            // create random exercise numbers
            Random random = new Random();
            List<int> numbers = new List<int>();
            for (int i = 0; i < amount; i++)
            {
                if (i >= exercisesDb.Count)
                {
                    break;
                }
                int number = random.Next(0, exercisesDb.Count);
                while (numbers.Contains(number))
                {
                    number = random.Next(0, exercisesDb.Count);
                }
                numbers.Add(number);
            }

            // get random exercises
            List<Exercise> exercises = new List<Exercise>();
            foreach (int number in numbers)
            {
                exercises.Add(exercisesDb[number]);
            }

            List<ExerciseResponse> response = new List<ExerciseResponse>();
            foreach (Exercise exercise in exercises)
            {
                // create response
                response.Add(new ExerciseResponse
                {
                    Id = exercise.Id,
                    Name = exercise.Name,
                    Media = exercise.Media,
                    Requirements = exercise.ExerciseRequirements.Select(r => r.Requirement).ToList(),
                    Steps = exercise.ExerciseSteps.Select(s => s.StepDescription).ToList(),
                    Forms = exercise.ExerciseForms.Select(f => f.MovementForm).ToList()
                });
            }

            return response;
        }
    }
}
