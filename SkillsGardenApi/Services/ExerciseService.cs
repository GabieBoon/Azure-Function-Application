using SkillsGardenApi.Models;
using SkillsGardenApi.Repositories;
using SkillsGardenDTO;
using SkillsGardenDTO.Response;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkillsGardenApi.Services
{
    public class ExerciseService
    {
        private ExerciseRepository exerciseRepository;
        private ComponentRepository componentRepository;

        public ExerciseService(IDatabaseRepository<Exercise> exerciseRepository, IDatabaseRepository<Component> componentRepository)
        {
            this.exerciseRepository = (ExerciseRepository)exerciseRepository;
            this.componentRepository = (ComponentRepository)componentRepository;
        }

        public async Task<List<ExerciseResponse>> GetAllExercises()
        {
            // get list of exercises
            List<Exercise> exercises = await exerciseRepository.ListAsync();

            // create response
            List<ExerciseResponse> response = new List<ExerciseResponse>();
            foreach (Exercise exercise in exercises)
            {
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

        public async Task<List<ExerciseResponse>> GetExercisesForComponent(int componentId)
        {
            // get component
            Component component = await this.componentRepository.ReadAsync(componentId);

            // get list of exercises for component
            List<int> exerciseIds = component.ComponentExercises.Select(e => e.ExerciseId).ToList();

            // create response
            List<ExerciseResponse> response = new List<ExerciseResponse>();
            foreach (int exerciseId in exerciseIds)
            {
                // get exercise
                Exercise exercise = await exerciseRepository.ReadAsync(exerciseId);

                if (exercise == null)
                    continue;

                // create response
                response.Add(new ExerciseResponse
                {
                    Id = exerciseId,
                    Name = exercise.Name,
                    Media = exercise.Media,
                    Requirements = exercise.ExerciseRequirements.Select(r => r.Requirement).ToList(),
                    Steps = exercise.ExerciseSteps.Select(s => s.StepDescription).ToList(),
                    Forms = exercise.ExerciseForms.Select(f => f.MovementForm).ToList()
                });
            }

            return response;
        }

        public async Task<ExerciseResponse> GetExerciseById(int exerciseId)
        {
            // get exercise
            Exercise exercise = await exerciseRepository.ReadAsync(exerciseId);

            // create response
            ExerciseResponse response = new ExerciseResponse
            {
                Id = exercise.Id,
                Name = exercise.Name,
                Media = exercise.Media,
                Requirements = exercise.ExerciseRequirements.Select(r => r.Requirement).ToList(),
                Steps = exercise.ExerciseSteps.Select(s => s.StepDescription).ToList(),
                Forms = exercise.ExerciseForms.Select(f => f.MovementForm).ToList()
            };

            return response;
        }

        public async Task<Exercise> CreateExercise(ExerciseBody exerciseBody)
        {
            // create exercise
            Exercise newExercise = new Exercise
            {
                Name = exerciseBody.Name,
                Media = exerciseBody.Media,
                ExerciseSteps = new List<ExerciseStep>(),
                ExerciseRequirements = new List<ExerciseRequirement>(),
                ExerciseForms = new List<ExerciseForm>()
            };

            // create exercise steps
            await CreateSteps(exerciseBody, newExercise);

            // create exercise requirements
            await CreateRequirements(exerciseBody, newExercise);

            // create exercise forms
            await CreateForms(exerciseBody, newExercise);

            // save exercise to database
            newExercise = await exerciseRepository.CreateAsync(newExercise);

            return newExercise;
        }

        public async Task<Exercise> UpdateExercise(int exerciseId, ExerciseBody exerciseBody)
        {
            // update exercise
            Exercise updatedExercise = new Exercise()
            {
                Id = exerciseId,
                Name = exerciseBody.Name,
                Media = exerciseBody.Media,
                ExerciseSteps = null,
                ExerciseRequirements = null,
                ExerciseForms = null
            };

            // create exercise requirements
            if (exerciseBody.Requirements != null) await CreateRequirements(exerciseBody, updatedExercise);

            // create exercise steps
            if (exerciseBody.Steps != null) await CreateSteps(exerciseBody, updatedExercise);

            // create exercise forms
            if (exerciseBody.Forms != null) await CreateForms(exerciseBody, updatedExercise);

            // save exercise to database
            Exercise exercise = await exerciseRepository.UpdateAsync(updatedExercise);

            return exercise;
        }

        public async Task<bool> Exists(int exerciseId)
        {
            return await this.exerciseRepository.ExerciseExists(exerciseId);
        }

        public async Task<bool> Delete(int exerciseId)
        {
            bool isDeletedR = await this.exerciseRepository.DeleteRequirementsAsync(exerciseId);
            bool isDeletedS = await this.exerciseRepository.DeleteStepsAsync(exerciseId);
            bool isDeletedF = await this.exerciseRepository.DeleteFormsAsync(exerciseId);
            bool isDeleted = await this.exerciseRepository.DeleteAsync(exerciseId);

            if (!isDeleted || !isDeletedR || !isDeletedS || !isDeletedF)
            {
                return false;
            }
            return true;
        }

        private async Task CreateRequirements(ExerciseBody exerciseBody, Exercise newExercise)
        {
            // delete old
            await exerciseRepository.DeleteRequirementsAsync(newExercise.Id);

            // create new
            newExercise.ExerciseRequirements = new List<ExerciseRequirement>();
            foreach (string requirement in exerciseBody.Requirements)
            {
                ExerciseRequirement newRequirement = new ExerciseRequirement
                {
                    Exercise = newExercise,
                    Requirement = requirement
                };
                newExercise.ExerciseRequirements.Add(newRequirement);
            }
        }

        private async Task CreateSteps(ExerciseBody exerciseBody, Exercise newExercise)
        {
            // delete old
            await exerciseRepository.DeleteStepsAsync(newExercise.Id);

            // create new
            newExercise.ExerciseSteps = new List<ExerciseStep>();
            int stepCount = 1;
            foreach (string step in exerciseBody.Steps)
            {
                ExerciseStep newStep = new ExerciseStep
                {
                    Exercise = newExercise,
                    StepNumber = stepCount,
                    StepDescription = step
                };
                newExercise.ExerciseSteps.Add(newStep);
                stepCount++;
            }
        }

        private async Task CreateForms(ExerciseBody exerciseBody, Exercise newExercise)
        {
            // delete old
            await exerciseRepository.DeleteFormsAsync(newExercise.Id);

            // create new
            newExercise.ExerciseForms = new List<ExerciseForm>();
            List<MovementForm> exerciseForms = new List<MovementForm>();
            foreach (MovementForm form in exerciseBody.Forms)
            {
                // if movement form was already added
                if (exerciseForms.Contains(form))
                {
                    continue;
                }
                ExerciseForm newForm = new ExerciseForm
                {
                    Exercise = newExercise,
                    MovementForm = form
                };
                newExercise.ExerciseForms.Add(newForm);
                exerciseForms.Add(newForm.MovementForm);
            }
        }
    }
}
