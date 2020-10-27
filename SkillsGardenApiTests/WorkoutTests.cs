using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using SkillsGardenApi.Controllers;
using SkillsGardenApi.Models;
using SkillsGardenApi.Repositories;
using SkillsGardenApi.Repositories.Context;
using SkillsGardenApi.Services;
using SkillsGardenApiTests.Factory;
using SkillsGardenDTO;
using SkillsGardenDTO.Error;
using SkillsGardenDTO.Response;
using System.Collections.Generic;
using Xunit;

namespace SkillsGardenApiTests
{
    public class WorkoutsTests : UnitTest
    {
        private ExerciseRepository exerciseRepository;
        private WorkoutRepository workoutRepository;
        private ComponentRepository componentRepository;

        private WorkoutService workoutService;
        private ExerciseService exerciseService;

        private WorkoutController workoutController;

        public WorkoutsTests()
        {
            DatabaseContext dbContext = CreateEmptyDatabase();

            // create empty database
            this.exerciseRepository = new ExerciseRepository(dbContext);
            this.workoutRepository = new WorkoutRepository(dbContext);
            this.componentRepository = new ComponentRepository(dbContext);

            // create sevice
            this.workoutService = new WorkoutService(this.exerciseRepository, this.workoutRepository);
            this.exerciseService = new ExerciseService(this.exerciseRepository, this.componentRepository);

            // create controller
            this.workoutController = new WorkoutController(this.workoutService, this.exerciseService);
        }

        [Fact]
        public async void GetWorkoutWithoutMovementFormsTest()
        {
            FillDatabase();

            // create query parameters
            Dictionary<string, StringValues> query = new Dictionary<string, StringValues>
            {
                { "amount", "5" },
                { "movementforms", "" }
            };

            HttpRequest request = HttpRequestFactory.CreateGetRequest(query);

            ObjectResult result = (ObjectResult)await this.workoutController.WorkoutGenerate(request);

            List<ExerciseResponse> exercises = (List<ExerciseResponse>)result.Value;

            // status code should be 200 OK
            Assert.Equal(200, result.StatusCode);
            // amount of found exercises should be 5
            Assert.Equal(5, exercises.Count);
        }

        [Fact]
        public async void GetWorkoutWithSpecificMovementFormTest()
        {
            FillDatabase(false);

            // create query parameters
            Dictionary<string, StringValues> query = new Dictionary<string, StringValues>
            {
                { "amount", "5" },
                { "movementforms", "balans" }
            };

            HttpRequest request = HttpRequestFactory.CreateGetRequest(query);

            ObjectResult result = (ObjectResult)await this.workoutController.WorkoutGenerate(request);

            List<ExerciseResponse> exercises = (List<ExerciseResponse>)result.Value;

            // status code should be 200 OK
            Assert.Equal(200, result.StatusCode);
            // amount of found exercises should be 2
            Assert.Equal(2, exercises.Count);
        }

        [Fact]
        public async void GetWorkoutWithSpecificMovementFormsTest()
        {
            FillDatabase(false);

            List<Exercise> exercisestest = await this.exerciseRepository.ListAsync();

            // create query parameters
            Dictionary<string, StringValues> query = new Dictionary<string, StringValues>
            {
                { "amount", "5" },
                { "movementforms", "balans|mikken" }
            };

            HttpRequest request = HttpRequestFactory.CreateGetRequest(query);

            ObjectResult result = (ObjectResult)await this.workoutController.WorkoutGenerate(request);

            List<ExerciseResponse> exercises = (List<ExerciseResponse>)result.Value;

            // status code should be 200 OK
            Assert.Equal(200, result.StatusCode);
            // amount of found exercises should be 3
            Assert.Equal(3, exercises.Count);
        }

        [Theory]
        [InlineData("invalidmovementform")]
        [InlineData("|")]
        public async void GetWorkoutWithInvalidMovementFormTest(string movementforms)
        {
            FillDatabase();

            // create query parameters
            Dictionary<string, StringValues> query = new Dictionary<string, StringValues>
            {
                { "amount", "2" },
                { "movementforms", movementforms }
            };

            HttpRequest request = HttpRequestFactory.CreateGetRequest(query);

            ObjectResult result = (ObjectResult)await this.workoutController.WorkoutGenerate(request);

            ErrorResponse errorResponse = (ErrorResponse)result.Value;

            // status code should be 400 BAD REQUEST
            Assert.Equal(400, result.StatusCode);
            // error code must be invalid movement form provided
            Assert.Equal(ErrorCode.INVALID_MOVEMENT_FORM_PROVIDED, errorResponse.ErrorCodeEnum);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 1)]
        [InlineData(2, 2)]
        [InlineData(3, 3)]
        [InlineData(4, 4)]
        [InlineData(5, 5)]
        [InlineData(6, 5)]
        public async void GetWorkoutAmountShouldBeBetweenOneAndFiveTest(int amount, int expected)
        {
            FillDatabase();

            // create query parameters
            Dictionary<string, StringValues> query = new Dictionary<string, StringValues>
            {
                { "amount", amount.ToString() },
                { "movementforms", "" }
            };

            HttpRequest request = HttpRequestFactory.CreateGetRequest(query);

            ObjectResult result = (ObjectResult)await this.workoutController.WorkoutGenerate(request);

            List<ExerciseResponse> exercises = (List<ExerciseResponse>)result.Value;

            // status code should be 200 OK
            Assert.Equal(200, result.StatusCode);
            // amount of found exercises should be the expected
            Assert.Equal(expected, exercises.Count);
        }

        [Fact]
        public async void GetWorkoutWithoutAmountQueryParameterTest()
        {
            FillDatabase();

            // create query parameters
            Dictionary<string, StringValues> query = new Dictionary<string, StringValues>
            {
                { "movementforms", "" }
            };

            HttpRequest request = HttpRequestFactory.CreateGetRequest(query);

            ObjectResult result = (ObjectResult)await this.workoutController.WorkoutGenerate(request);

            List<ExerciseResponse> exercises = (List<ExerciseResponse>)result.Value;

            // status code should be 200 OK
            Assert.Equal(200, result.StatusCode);
            // amount of found exercises should be 3
            Assert.Equal(3, exercises.Count);
        }

        [Fact]
        public async void GetWorkoutWithoutMovementformsQueryParameterTest()
        {
            FillDatabase();

            // create query parameters
            Dictionary<string, StringValues> query = new Dictionary<string, StringValues>
            {
                { "amount", "5" }
            };

            HttpRequest request = HttpRequestFactory.CreateGetRequest(query);

            ObjectResult result = (ObjectResult)await this.workoutController.WorkoutGenerate(request);

            List<ExerciseResponse> exercises = (List<ExerciseResponse>)result.Value;

            // status code should be 200 OK
            Assert.Equal(200, result.StatusCode);
            // amount of found exercises should be 5
            Assert.Equal(5, exercises.Count);
        }

        private async void FillDatabase(bool withNonRealExercises = true)
        {
            // create first exercise
            Exercise exercise1 = ExerciseFactory.CreateExercise("Touwtje springen");
            exercise1.ExerciseRequirements = new List<ExerciseRequirement>()
            {
                ExerciseFactory.CreateExerciseRequirement("Touw")
            };
            exercise1.ExerciseSteps = new List<ExerciseStep>()
            {
                ExerciseFactory.CreateExerciseStep(1, "Spring 30 keer"),
                ExerciseFactory.CreateExerciseStep(2, "Rust"),
                ExerciseFactory.CreateExerciseStep(3, "Spring nog eens 30 keer")
            };
            exercise1.ExerciseForms = new List<ExerciseForm>()
            {
                ExerciseFactory.CreateExerciseForm(MovementForm.springen),
                ExerciseFactory.CreateExerciseForm(MovementForm.balans)
            };
            await this.exerciseRepository.CreateAsync(exercise1);

            // create second exercise
            Exercise exercise2 = ExerciseFactory.CreateExercise("Voetballen");
            exercise2.ExerciseRequirements = new List<ExerciseRequirement>()
            {
                ExerciseFactory.CreateExerciseRequirement("Voetbal"),
                ExerciseFactory.CreateExerciseRequirement("Voetbalschoenen")
            };
            exercise2.ExerciseSteps = new List<ExerciseStep>()
            {
                ExerciseFactory.CreateExerciseStep(1, "Houd 40 keer hoog"),
                ExerciseFactory.CreateExerciseStep(2, "Rust"),
                ExerciseFactory.CreateExerciseStep(3, "Probeer je record te verbreken")
            };
            exercise2.ExerciseForms = new List<ExerciseForm>()
            {
                ExerciseFactory.CreateExerciseForm(MovementForm.mikken),
                ExerciseFactory.CreateExerciseForm(MovementForm.balans)
            };
            await this.exerciseRepository.CreateAsync(exercise2);

            // create third exercise
            Exercise exercise3 = ExerciseFactory.CreateExercise("Ping pong");
            exercise3.ExerciseRequirements = new List<ExerciseRequirement>()
            {
                ExerciseFactory.CreateExerciseRequirement("Ping pong batjes"),
                ExerciseFactory.CreateExerciseRequirement("Ping pong balletje")
            };
            exercise3.ExerciseSteps = new List<ExerciseStep>()
            {
                ExerciseFactory.CreateExerciseStep(1, "Probeer de ping pong bal 20 keer hoog te houden"),
                ExerciseFactory.CreateExerciseStep(2, "Probeer het nog eens"),
                ExerciseFactory.CreateExerciseStep(3, "Ben je niet alleen? Probeer dan een 1 v 1")
            };
            exercise3.ExerciseForms = new List<ExerciseForm>()
            {
                ExerciseFactory.CreateExerciseForm(MovementForm.mikken)
            };
            await this.exerciseRepository.CreateAsync(exercise3);

            if (withNonRealExercises)
            {
                // create exercise without requirements
                Exercise exercise4 = ExerciseFactory.CreateExercise("Exercise zonder benodigdheden");
                exercise4.ExerciseRequirements = new List<ExerciseRequirement>() { };
                exercise4.ExerciseSteps = new List<ExerciseStep>()
                {
                    ExerciseFactory.CreateExerciseStep(1, "Step1")
                };
                exercise4.ExerciseForms = new List<ExerciseForm>()
                {
                    ExerciseFactory.CreateExerciseForm(MovementForm.springen)
                };
                await this.exerciseRepository.CreateAsync(exercise4);

                // create exercise without steps
                Exercise exercise5 = ExerciseFactory.CreateExercise("Exercise zonder stappen");
                exercise5.ExerciseRequirements = new List<ExerciseRequirement>()
                {
                    ExerciseFactory.CreateExerciseRequirement("Requirement1")
                };
                exercise5.ExerciseSteps = new List<ExerciseStep>() { };
                exercise5.ExerciseForms = new List<ExerciseForm>()
                {
                    ExerciseFactory.CreateExerciseForm(MovementForm.springen)
                };
                await this.exerciseRepository.CreateAsync(exercise5);

                // create exercise without movement forms
                Exercise exercise6 = ExerciseFactory.CreateExercise("Exercise zonder beweegvormen");
                exercise6.ExerciseRequirements = new List<ExerciseRequirement>()
                {
                    ExerciseFactory.CreateExerciseRequirement("Requirement1")
                };
                exercise6.ExerciseSteps = new List<ExerciseStep>()
                {
                    ExerciseFactory.CreateExerciseStep(1, "Step1")
                };
                exercise6.ExerciseForms = new List<ExerciseForm>() { };
                await this.exerciseRepository.CreateAsync(exercise6);
            }
        }
    }
}
