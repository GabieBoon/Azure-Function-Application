using AzureFunctions.Extensions.Swashbuckle.Attribute;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Spatial;
using Newtonsoft.Json;
using SkillsGardenApi.Models;
using SkillsGardenApi.Services;
using SkillsGardenApi.Utils;
using SkillsGardenDTO;
using SkillsGardenDTO.Error;
using SkillsGardenDTO.Response;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SkillsGardenApi.Controllers
{
    public class WorkoutController
    {
        private WorkoutService workoutService;
        private ExerciseService exerciseService;

        public WorkoutController(WorkoutService workoutService, ExerciseService exerciseService)
        {
            this.workoutService = workoutService;
            this.exerciseService = exerciseService;
        }

        /// <summary>
        /// Get all workouts
        /// </summary>
        [FunctionName("WorkoutGetAll")]
        [ProducesResponseType(typeof(List<WorkoutListResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> WorkoutGetAll(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "workouts")] HttpRequest req)
        {
            // get the workouts
            List<WorkoutListResponse> workouts = await workoutService.GetWorkouts();

            return new OkObjectResult(workouts);
        }

        /// <summary>
        /// Get one workout
        /// </summary>
        [FunctionName("WorkoutGet")]
        [ProducesResponseType(typeof(Workout), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> WorkoutGet(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "workouts/{workoutId}")] HttpRequest req,
            int workoutId)
        {
            // get the workout
            WorkoutResponse workout = await workoutService.GetWorkout(workoutId);

            // check if requested workout exists
            if (workout == null)
                return new NotFoundObjectResult(new ErrorResponse(ErrorCode.WORKOUT_NOT_FOUND));

            return new OkObjectResult(workout);
        }

        /// <summary>
        /// Create workout
        /// </summary>
        [FunctionName("WorkoutCreate")]
        [ProducesResponseType(typeof(WorkoutResponse), StatusCodes.Status200OK)]   
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> WorkoutCreate(
            [HttpTrigger(AuthorizationLevel.User, "post", Route = "workouts")]
            [RequestBodyType(typeof(WorkoutBody), "The workout to create")] HttpRequest req,
            [SwaggerIgnore] ClaimsPrincipal user)
        {
            // check if user has admin rights 
            if (!user.IsInRole(UserType.Admin.ToString()))
                return ForbiddenObjectResult.Create(new ErrorResponse(ErrorCode.UNAUTHORIZED_ROLE_NO_PERMISSIONS));

            // deserialize request
            WorkoutBody workoutBody;
            try {
                workoutBody = await SerializationUtil.Deserialize<WorkoutBody>(req.Body);
            }
            catch (JsonException e) {
                return new BadRequestObjectResult(new ErrorResponse(400, e.Message));
            }

            // check if all fields are filled in
            if (workoutBody.Name == null || workoutBody.Type == null || workoutBody.Exercises == null || workoutBody.Exercises.Count == 0)
                return new BadRequestObjectResult(new ErrorResponse(ErrorCode.INVALID_REQUEST_BODY));

            // check if given exercises exist
            foreach (int exerciseId in workoutBody.Exercises)
            {
                if (!await exerciseService.Exists(exerciseId))
                    return new BadRequestObjectResult(new ErrorResponse(ErrorCode.INVALID_EXERCISE_PROVIDED));
            }

            // create new workout
            int workoutId = await workoutService.CreateWorkout(workoutBody);

            // get the created location
            WorkoutResponse createdWorkout = await workoutService.GetWorkout(workoutId);

            return new OkObjectResult(createdWorkout);
        }

        /// <summary>
        /// Update workout
        /// </summary>
        [FunctionName("WorkoutUpdate")]
        [ProducesResponseType(typeof(WorkoutResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> WorkoutUpdate(
            [HttpTrigger(AuthorizationLevel.User, "put", Route = "workouts/{workoutId}")]
            [RequestBodyType(typeof(WorkoutBody), "The workout to update")] HttpRequest req,
            int workoutId,
            [SwaggerIgnore] ClaimsPrincipal user)
        {
            // check if user has admin rights 
            if (!user.IsInRole(UserType.Admin.ToString()))
                return ForbiddenObjectResult.Create(new ErrorResponse(ErrorCode.UNAUTHORIZED_ROLE_NO_PERMISSIONS));

            // check if requested workout exists
            if (!await workoutService.Exists(workoutId))
                return new NotFoundObjectResult(new ErrorResponse(ErrorCode.WORKOUT_NOT_FOUND));

            // deserialize request
            WorkoutBody workoutBody;
            try
            {
                workoutBody = await SerializationUtil.Deserialize<WorkoutBody>(req.Body);
            }
            catch (JsonException e)
            {
                return new BadRequestObjectResult(new ErrorResponse(400, e.Message));
            }

            // update workout
            await workoutService.UpdateWorkout(workoutBody, workoutId);

            // get the updated workout
            WorkoutResponse updatedWorkout = await workoutService.GetWorkout(workoutId);

            return new OkObjectResult(updatedWorkout);
        }

        /// <summary>
        /// Delete workout
        /// </summary>
        [FunctionName("WorkoutDelete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> WorkoutDelete(
            [HttpTrigger(AuthorizationLevel.User, "delete", Route = "workouts/{workoutId}")] HttpRequest req,
            int workoutId,
            [SwaggerIgnore] ClaimsPrincipal user)
        {
            // only admin can delete workout
            if (!user.IsInRole(UserType.Admin.ToString()))
                return ForbiddenObjectResult.Create(new ErrorResponse(ErrorCode.UNAUTHORIZED_ROLE_NO_PERMISSIONS));

            // delete workout
            bool isDeleted = await workoutService.DeleteWorkout(workoutId);

            // if the workout was not found
            if (!isDeleted)
                return new NotFoundObjectResult(new ErrorResponse(ErrorCode.WORKOUT_NOT_FOUND));

            return new OkResult();
        }

        /// <summary>
        /// Generate workout
        /// </summary>
        [FunctionName("WorkoutGenerate")]
        [ProducesResponseType(typeof(List<ExerciseResponse>), StatusCodes.Status200OK)]
        [QueryStringParameter("amount", "Amount of exercises in this workout", DataType = typeof(int), Required = false)]
        [QueryStringParameter("movementforms", "Movement forms seperated with a pipe (|) sign", DataType = typeof(string), Required = false)]
        public async Task<IActionResult> WorkoutGenerate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "workouts/generate")] HttpRequest req)
        {
            // get amount
            int amount = req.Query.ContainsKey("amount") ? Int32.Parse(req.Query["amount"]) : 3;
            if (amount > 5) amount = 5;
            if (amount < 1) amount = 1;

            // get movement forms
            string movementFormsPlain = req.Query.ContainsKey("movementforms") ? req.Query["movementforms"].ToString() : "";
            List<MovementForm> movementForms;
            try {
                movementForms = this.workoutService.ConvertToMovementformList(movementFormsPlain);
            }
            catch (ParseErrorException) {
                return new BadRequestObjectResult(new ErrorResponse(ErrorCode.INVALID_MOVEMENT_FORM_PROVIDED));
            }

            // get exercises for the movement forms
            List<ExerciseResponse> exercises = await this.workoutService.GetExercisesByMovementforms(amount, movementForms);

            return new OkObjectResult(exercises);
        }
    }
}
