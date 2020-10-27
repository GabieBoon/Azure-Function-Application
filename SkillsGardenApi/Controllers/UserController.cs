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
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace SkillsGardenApi.Controllers
{
    public class UserController
    {
        private UserService userService;
        private BeaconService beaconService;
        private EventService eventService;

        public UserController(UserService userService, BeaconService beaconService, EventService eventService)
        {
            this.userService = userService;
            this.beaconService = beaconService;
            this.eventService = eventService;
        }

        [FunctionName("UsersGetAll")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UsersGetAll(
            [HttpTrigger(AuthorizationLevel.User, "get", Route = "users")] HttpRequest req,
            [SwaggerIgnore] ClaimsPrincipal userClaim)
        {
            // user must be admin
            if (!userClaim.IsInRole(UserType.Admin.ToString()))
                return ForbiddenObjectResult.Create(new ErrorResponse(ErrorCode.UNAUTHORIZED_ROLE_NO_PERMISSIONS));

            // get list of users
            List<UserResponse> users = await userService.GetAllUsers();

            return new OkObjectResult(users);
        }

        [FunctionName("UsersGet")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UsersGet(
            [HttpTrigger(AuthorizationLevel.User, "get", Route = "users/{userId}")] HttpRequest req,
            int userId,
            [SwaggerIgnore] ClaimsPrincipal userClaim)
        {
            // non-admin can only get own account
            if (!userClaim.IsInRole(UserType.Admin.ToString()) && Int32.Parse(userClaim.FindFirstValue(ClaimTypes.NameIdentifier)) != userId)
                return ForbiddenObjectResult.Create(new ErrorResponse(ErrorCode.CAN_ONLY_VIEW_OWN_ACCOUNT));

            // get the user
            UserResponse user = await userService.GetUser(userId);

            // if the user was not found
            if (user == null)
                return new NotFoundObjectResult(new ErrorResponse(ErrorCode.USER_NOT_FOUND));

            return new OkObjectResult(user);
        }

        [FunctionName("UserCreate")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UserCreate(
            [HttpTrigger(AuthorizationLevel.User, "post", Route = "users")]
            [RequestBodyType(typeof(UserBody), "The user to create")] HttpRequest req,
            [SwaggerIgnore] ClaimsPrincipal userClaim)
        {
            // deserialize request
            UserBody userBody;
            try {
                userBody = await SerializationUtil.Deserialize<UserBody>(req.Body);
            } catch (JsonException e) {
                return new BadRequestObjectResult(new ErrorResponse(400, e.Message));
            }

            // check for required fields
            if (userBody.Name == null) return new BadRequestObjectResult(new ErrorResponse(400, "Name is required"));
            if (userBody.Email == null) return new BadRequestObjectResult(new ErrorResponse(400, "Email is required"));
            if (userBody.Password == null) return new BadRequestObjectResult(new ErrorResponse(400, "Password is required"));
            if (userBody.Dateofbirth == null) return new BadRequestObjectResult(new ErrorResponse(400, "Dateofbirth is required"));
            if (userBody.Gender == null) return new BadRequestObjectResult(new ErrorResponse(400, "Gender is required"));
            if (userBody.Type == null) return new BadRequestObjectResult(new ErrorResponse(400, "Type is required"));

            // check for date of birth not in the future    
            if (userBody.Dateofbirth >= DateTime.Now) return new BadRequestObjectResult(new ErrorResponse(400, "Dateofbirth can not be in the future"));

            // only admin can set specific user type
            if (!userClaim.IsInRole(UserType.Admin.ToString()))
            {
                if (userBody.Type != null && userBody.Type != UserType.User)
                    return ForbiddenObjectResult.Create(new ErrorResponse(ErrorCode.UNAUTHORIZED_TO_SET_USER_TYPE));
            }

            // check if there is already an user with this email
            if (!await userService.EmailExists(userBody.Email))
                return new BadRequestObjectResult(new ErrorResponse(400, "Email does already exist"));

            // create user
            User createdUser = await this.userService.CreateUser(userBody);

            // get the created user
            UserResponse response = await userService.GetUser(createdUser.Id);

            return new OkObjectResult(response);
        }

        [FunctionName("UserUpdate")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UserUpdate(
            [HttpTrigger(AuthorizationLevel.User, "put", Route = "users/{userId}")]
            [RequestBodyType(typeof(UserBody), "The user to update")] HttpRequest req,
            int userId,
            [SwaggerIgnore] ClaimsPrincipal userClaim)
        {
            // non-admin can only edit own account
            if (!userClaim.IsInRole(UserType.Admin.ToString()) && Int32.Parse(userClaim.FindFirstValue(ClaimTypes.NameIdentifier)) != userId)
                return ForbiddenObjectResult.Create(new ErrorResponse(ErrorCode.CAN_ONLY_EDIT_OWN_ACCOUNT));

            // deserialize request
            UserBody userBody;
            try {
                userBody = await SerializationUtil.Deserialize<UserBody>(req.Body);
            } catch (JsonException e) {
                return new BadRequestObjectResult(new ErrorResponse(400, e.Message));
            }

            // only admin can edit specific user type
            if (!userClaim.IsInRole(UserType.Admin.ToString()))
            {
                if (userBody.Type != null && userBody.Type != UserType.User)
                    return ForbiddenObjectResult.Create(new ErrorResponse(ErrorCode.UNAUTHORIZED_TO_SET_USER_TYPE));
            }

            // update user
            User updatedUser = await this.userService.UpdateUser(userId, userBody);

            // when user was not found
            if (updatedUser == null)
                return new NotFoundObjectResult(new ErrorResponse(ErrorCode.USER_NOT_FOUND));

            // get the updated user
            UserResponse response = await userService.GetUser(updatedUser.Id);

            return new OkObjectResult(response);
        }

        [FunctionName("UserDelete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UserDelete(
            [HttpTrigger(AuthorizationLevel.User, "delete", Route = "users/{userId}")] HttpRequest req,
            int userId,
            [SwaggerIgnore] ClaimsPrincipal userClaim)
        {
            // only admin can delete user
            if (!userClaim.IsInRole(UserType.Admin.ToString()))
                return ForbiddenObjectResult.Create(new ErrorResponse(ErrorCode.UNAUTHORIZED_TO_DELETE_USER));
                
            // delete user
            bool isDeleted = await userService.DeleteUser(userId);

            // if user was not found
            if (!isDeleted)
                return new NotFoundObjectResult(new ErrorResponse(ErrorCode.USER_NOT_FOUND));

            return new OkResult();
        }

        [FunctionName("UsersGetAllBeaconLogs")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UsersGetAllBeaconLogs(
            [HttpTrigger(AuthorizationLevel.User, "get", Route = "users/{userId}/beacons")] HttpRequest req,
            int userId, 
            [SwaggerIgnore] ClaimsPrincipal userClaim)
        {
            int userClaimId = Int32.Parse(userClaim.FindFirstValue(ClaimTypes.NameIdentifier));

            if (userClaimId != userId)
                return new BadRequestObjectResult(new ErrorResponse(ErrorCode.GET_ONLY_LOG_YOURSELF));

            // get list of own beaconLogs
            List<BeaconLog> beaconLogs = await beaconService.GetBeaconLogsByUserId(userId);

            return new OkObjectResult(beaconLogs);
        }

        [FunctionName("UserBeaconLog")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UserBeaconLog(
            [HttpTrigger(AuthorizationLevel.User, "post", Route = "users/{userId}/beacons/{beaconId}")] HttpRequest req,
            int userId, int beaconId,
            [SwaggerIgnore] ClaimsPrincipal userClaim)
        {
            int userClaimId = Int32.Parse(userClaim.FindFirstValue(ClaimTypes.NameIdentifier));

            if (userClaimId != userId)
                return new NotFoundObjectResult(new ErrorResponse(ErrorCode.ONLY_LOG_YOURSELF));

            bool logged = await this.beaconService.LogUser(userId, beaconId);

            if (!logged)
                return new NotFoundObjectResult(new ErrorResponse(ErrorCode.BEACON_NOT_FOUND));

            return new OkResult();
        }

        [FunctionName("UserDeleteBeaconLogs")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UserDeleteBeaconLogs(
            [HttpTrigger(AuthorizationLevel.User, "delete", Route = "users/{userId}/beacons")] HttpRequest req,
            int userId,
            [SwaggerIgnore] ClaimsPrincipal userClaim)
        {
            int userClaimId = Int32.Parse(userClaim.FindFirstValue(ClaimTypes.NameIdentifier));

            if (userClaimId != userId)
                return new NotFoundObjectResult(new ErrorResponse(ErrorCode.ONLY_DELETE_OWN_LOGS));

            // delete user
            bool isDeleted = await beaconService.DeleteBeaconLog(userId);

            // if user was not found
            if (!isDeleted)
                return new NotFoundObjectResult(new ErrorResponse(ErrorCode.USER_NOT_FOUND));

            return new OkResult();
        }

        [FunctionName("UserGetEventRegistrations")]
        [ProducesResponseType(typeof(List<EventResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UserGetEventRegistrations(
            [HttpTrigger(AuthorizationLevel.User, "get", Route = "users/{userId}/events")] HttpRequest req,
            int userId,
            [SwaggerIgnore] ClaimsPrincipal userClaim)
        {
            // non-admin can only view own registrations
            if (!userClaim.IsInRole(UserType.Admin.ToString()) && Int32.Parse(userClaim.FindFirstValue(ClaimTypes.NameIdentifier)) != userId)
                return ForbiddenObjectResult.Create(new ErrorResponse(ErrorCode.CAN_ONLY_VIEW_OWN_REGISTRATIONS));

            List<EventResponse> response = await eventService.GetUserRegisteredEvents(userId);

            return new OkObjectResult(response);
        }
    }
}
