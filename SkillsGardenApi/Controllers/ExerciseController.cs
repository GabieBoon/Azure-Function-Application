using AzureFunctions.Extensions.Swashbuckle.Attribute;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using SkillsGardenApi.Models;
using SkillsGardenApi.Services;
using SkillsGardenApi.Utils;
using SkillsGardenDTO;
using SkillsGardenDTO.Error;
using SkillsGardenDTO.Response;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace SkillsGardenApi.Controllers
{
    public class ExerciseController
    {
        private ExerciseService exerciseService;
        private LocationService locationService;
        private ComponentService componentService;

        public ExerciseController(ExerciseService exerciseService, LocationService locationService, ComponentService componentService)
        {
            this.exerciseService = exerciseService;
            this.locationService = locationService;
            this.componentService = componentService;
        }

        [FunctionName("ExercisesGetAll")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ExercisesGetAll(
            [HttpTrigger(AuthorizationLevel.User, "get", Route = "exercises")] HttpRequest req,
            [SwaggerIgnore] ClaimsPrincipal userClaim)
        {
            // user must be admin
            if (!userClaim.IsInRole(UserType.Admin.ToString()))
                return ForbiddenObjectResult.Create(new ErrorResponse(ErrorCode.UNAUTHORIZED_ROLE_NO_PERMISSIONS));

            // get list of exercises
            List<ExerciseResponse> exercises = await exerciseService.GetAllExercises();

            return new OkObjectResult(exercises);
        }

        [FunctionName("ExercisesGetAllByComponent")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ExercisesGetAllByComponent(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "locations/{locationId}/components/{componentId}/exercises")] HttpRequest req,
            int locationId, int componentId)
        {
            // if location does not exist
            if (!await locationService.Exists(locationId))
                return new NotFoundObjectResult(new ErrorResponse(ErrorCode.LOCATION_NOT_FOUND));

            // if component does not exist within location
            if (!await componentService.Exists(locationId, componentId))
                return new NotFoundObjectResult(new ErrorResponse(ErrorCode.COMPONENT_NOT_FOUND));

            // get exercises for component
            List<ExerciseResponse> exercises = await this.exerciseService.GetExercisesForComponent(componentId);

            return new OkObjectResult(exercises);
        }

        [FunctionName("ExercisesGet")]
        [ProducesResponseType(typeof(ExerciseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ExercisesGet(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "exercises/{exerciseId}")] HttpRequest req,
            int exerciseId)
        {
            // if the exercise does not exist
            if (!await this.exerciseService.Exists(exerciseId))
                return new NotFoundObjectResult(new ErrorResponse(ErrorCode.EXERCISE_NOT_FOUND));

            // get exercise
            ExerciseResponse exercise = await exerciseService.GetExerciseById(exerciseId);

            return new OkObjectResult(exercise);
        }

        [FunctionName("ExercisesCreate")]
        [ProducesResponseType(typeof(ExerciseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ExercisesCreate(
            [HttpTrigger(AuthorizationLevel.User, "post", Route = "exercises")]
            [RequestBodyType(typeof(ExerciseBody), "The exercise to create")] HttpRequest req,
            [SwaggerIgnore] ClaimsPrincipal user)
        {
            // user must be admin
            if (!user.IsInRole(UserType.Admin.ToString()))
                return ForbiddenObjectResult.Create(new ErrorResponse(ErrorCode.UNAUTHORIZED_ROLE_NO_PERMISSIONS));

            // deserialize request
            ExerciseBody exerciseBody;
            try {
                exerciseBody = await SerializationUtil.Deserialize<ExerciseBody>(req.Body);
            } catch (JsonException e) {
                return new BadRequestObjectResult(new ErrorResponse(400, e.Message));
            }

            // create exercise
            Exercise createdExercise = await this.exerciseService.CreateExercise(exerciseBody);

            // get exercise
            ExerciseResponse response = await this.exerciseService.GetExerciseById(createdExercise.Id);

            return new OkObjectResult(response);
        }

        [FunctionName("ExercisesUpdate")]
        [ProducesResponseType(typeof(ExerciseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ExercisesUpdate(
            [HttpTrigger(AuthorizationLevel.User, "put", Route = "exercises/{exerciseId}")]
            [RequestBodyType(typeof(ExerciseBody), "The exercise to update")] HttpRequest req,
            int exerciseId,
            [SwaggerIgnore] ClaimsPrincipal userClaim)
        {
            // user must be admin
            if (!userClaim.IsInRole(UserType.Admin.ToString()))
                return ForbiddenObjectResult.Create(new ErrorResponse(ErrorCode.UNAUTHORIZED_ROLE_NO_PERMISSIONS));

            // check if exercise exists
            if (!await this.exerciseService.Exists(exerciseId))
                return new NotFoundObjectResult(new ErrorResponse(ErrorCode.EXERCISE_NOT_FOUND));

            // deserialize request
            ExerciseBody exerciseBody;
            try{
                exerciseBody = await SerializationUtil.Deserialize<ExerciseBody>(req.Body);
            }
            catch (JsonException e) {
                return new BadRequestObjectResult(new ErrorResponse(400, e.Message));
            }

            // update exercise
            Exercise exercise = await this.exerciseService.UpdateExercise(exerciseId, exerciseBody);

            // get exercise
            ExerciseResponse response = await this.exerciseService.GetExerciseById(exercise.Id);

            return new OkObjectResult(response);
        }

        [FunctionName("ExercisesDelete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ExercisesDelete(
            [HttpTrigger(AuthorizationLevel.User, "delete", Route = "exercises/{exerciseId}")] HttpRequest req,
            int exerciseId,
            [SwaggerIgnore] ClaimsPrincipal userClaim)
        {
            // only admin can delete exercise
            if (!userClaim.IsInRole(UserType.Admin.ToString()))
                return ForbiddenObjectResult.Create(new ErrorResponse(ErrorCode.UNAUTHORIZED_ROLE_NO_PERMISSIONS));

            // delete exercise
            bool exerciseDeleted = await exerciseService.Delete(exerciseId);

            // if exercise was not found
            if (!exerciseDeleted)
                return new NotFoundObjectResult(new ErrorResponse(ErrorCode.EXERCISE_NOT_FOUND));

            return new OkResult();
        }
    }
}
