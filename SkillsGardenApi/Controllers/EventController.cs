using AzureFunctions.Extensions.Swashbuckle.Attribute;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using SkillsGardenApi.Filters;
using SkillsGardenApi.Models;
using SkillsGardenApi.Services;
using SkillsGardenApi.Utils;
using SkillsGardenDTO;
using SkillsGardenDTO.Error;
using SkillsGardenDTO.Response;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SkillsGardenApi.Controllers
{
    public class EventController
    {
        private EventService eventService;
        private LocationService locationService;

        public EventController(EventService eventService, LocationService locationService)
        {
            this.eventService = eventService;
            this.locationService = locationService;
        }

        /// <summary>
        /// Get all events for a specific location
        /// </summary>
        [FunctionName("LocationGetEvents")]
        [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [QueryStringParameter("all", "Whether to include events in the past", DataType = typeof(bool), Required = false)]
        public async Task<IActionResult> LocationGetEvents(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "locations/{locationId}/events")] HttpRequest req,
            int locationId)
        {
            // if location does not exist
            if (!await locationService.Exists(locationId))
                return new NotFoundObjectResult(new ErrorResponse(ErrorCode.LOCATION_NOT_FOUND));

            // get query parameter
            bool all = req.Query.ContainsKey("all") ? Boolean.Parse(req.Query["all"]) : false;

            // get the events within the location
            List<EventResponse> events = await eventService.GetEventsByLocation(locationId, all);

            return new OkObjectResult(events);
        }

        /// <summary>
        /// Create a new event
        /// </summary>
        [FunctionName("LocationCreateEvent")]
        [FormDataItem(Name = "Title", Description = "The title the event", Type = "string")]
        [FormDataItem(Name = "Description", Description = "The description of the event", Type = "string")]
        [FormDataItem(Name = "StartTime", Description = "The starttime of the event", Type = "DateTime")]
        [FormDataItem(Name = "MaxRegistrations", Description = "The max amount of registrations for the event", Type = "int")]
        [FormDataItem(Name = "Image", Description = "The image of the event", Format = "binary", Type = "file")]
        [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> LocationCreateEvent(
            [HttpTrigger(AuthorizationLevel.User, "post", Route = "locations/{locationId}/events")]
            [RequestBodyType(typeof(EventBody), "Event to Create")] HttpRequest req,
            int locationId, [SwaggerIgnore] ClaimsPrincipal user)
        {
            // check if user is admin or organiser
            if (!user.IsInRole(UserType.Admin.ToString()) && !user.IsInRole(UserType.Organiser.ToString()))
                return ForbiddenObjectResult.Create(new ErrorResponse(ErrorCode.UNAUTHORIZED_ROLE_NO_PERMISSIONS));

            // if location does not exist
            if (!await locationService.Exists(locationId))
                return new NotFoundObjectResult(new ErrorResponse(ErrorCode.LOCATION_NOT_FOUND));

            // get the form data
            IFormCollection formdata = await req.ReadFormAsync();
            EventBody eventBody;
            try {
                eventBody = SerializationUtil.DeserializeFormData<EventBody>(formdata);
            }
            catch (ValidationException e) {
                return new BadRequestObjectResult(new ErrorResponse(400, e.Message));
            }

            // check if all fields are filled in
            if (eventBody.Title == null || eventBody.Description == null || eventBody.StartTime == null || eventBody.MaxRegistrations == null || eventBody.Image == null)
                return new BadRequestObjectResult(new ErrorResponse(ErrorCode.INVALID_REQUEST_BODY));

            // create new event
            int organiserId = Int32.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier));
            int eventId = await eventService.CreateEvent(eventBody, locationId, organiserId);

            // get the created event
            EventResponse newEvent = await eventService.GetEvent(locationId, eventId);

            return new OkObjectResult(newEvent);
        }

        /// <summary>
        /// Get a specific event for specific location
        /// </summary>
        [FunctionName("LocationGetEvent")]
        [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> LocationGetEvent(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "locations/{locationId}/events/{eventId}")] HttpRequest req,
            int locationId, int eventId)
        {
            // check if given location exists
            if (!await locationService.Exists(locationId))
                return new NotFoundObjectResult(new ErrorResponse(ErrorCode.LOCATION_NOT_FOUND));

            // get event
            EventResponse specificEvent = await eventService.GetEvent(locationId, eventId);

            // if event could not be found
            if (specificEvent == null)
                return new NotFoundObjectResult(new ErrorResponse(ErrorCode.EVENT_NOT_FOUND));

            return new OkObjectResult(specificEvent);
        }

        /// <summary>
        /// Edit an existing Event
        /// </summary>
        [FunctionName("LocationEditEvent")]
        [FormDataItem(Name = "Title", Description = "The title the event", Type = "string")]
        [FormDataItem(Name = "Description", Description = "The description of the event", Type = "string")]
        [FormDataItem(Name = "StartTime", Description = "The starttime of the event", Type = "DateTime")]
        [FormDataItem(Name = "MaxRegistrations", Description = "The max amount of registrations for the event", Type = "int")]
        [FormDataItem(Name = "Image", Description = "The image of the event", Format = "binary", Type = "file")]
        [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> LocationEditEvent(
            [HttpTrigger(AuthorizationLevel.User, "put", Route = "locations/{locationId}/events/{eventId}")]
            [RequestBodyType(typeof(EventBody), "Event to edit")] HttpRequest req,
            int locationId, int eventId, [SwaggerIgnore] ClaimsPrincipal user)
        {
            // check if user has admin rights 
            if (!user.IsInRole(UserType.Admin.ToString()))
                return ForbiddenObjectResult.Create(new ErrorResponse(ErrorCode.UNAUTHORIZED_ROLE_NO_PERMISSIONS));

            // check if given location exists
            if (!await locationService.Exists(locationId))
                return new NotFoundObjectResult(new ErrorResponse(ErrorCode.LOCATION_NOT_FOUND));

            // get the form data
            IFormCollection formdata = await req.ReadFormAsync();
            EventBody eventBody;
            try
            {
                eventBody = SerializationUtil.DeserializeFormData<EventBody>(formdata);
            }
            catch (ValidationException e)
            {
                return new BadRequestObjectResult(new ErrorResponse(400, e.Message));
            }

            // update event
            Event updatedEvent = await eventService.UpdateEvent(eventBody, locationId, eventId);

            // get the updated event
            EventResponse response = await eventService.GetEvent(locationId, eventId);

            // if event was not found
            if (updatedEvent == null || response == null)
                return new NotFoundObjectResult(new ErrorResponse(ErrorCode.EVENT_NOT_FOUND));

            return new OkObjectResult(response);
        }

        /// <summary>
        /// Delete an existing event
        /// </summary>
        [FunctionName("LocationDeleteEvent")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> LocationDeleteEvent(
            [HttpTrigger(AuthorizationLevel.User, "delete", Route = "locations/{locationId}/events/{eventId}")] HttpRequest req,
            int locationId, int eventId, [SwaggerIgnore] ClaimsPrincipal user)
        {
            // check if user has admin rights 
            if (!user.IsInRole(UserType.Admin.ToString()))
                return ForbiddenObjectResult.Create(new ErrorResponse(ErrorCode.UNAUTHORIZED_ROLE_NO_PERMISSIONS));

            // if location was not found
            if (!await locationService.Exists(locationId))
                return new NotFoundObjectResult(new ErrorResponse(ErrorCode.LOCATION_NOT_FOUND));

            // if event was not found within location
            if (!await eventService.Exists(locationId, eventId))
                return new NotFoundObjectResult(new ErrorResponse(ErrorCode.EVENT_NOT_FOUND));

            // delete event
            bool isDeleted = await eventService.DeleteEvent(eventId);

            // if the event was not deleted
            if (!isDeleted)
                return new BadRequestObjectResult(new ErrorResponse(ErrorCode.EVENT_DELETE_FAILED));

            return new OkResult();
        }

        /// <summary>
        /// Register for an event
        /// </summary>
        [FunctionName("LocationCreateRegistration")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> LocationCreateRegistration(
            [HttpTrigger(AuthorizationLevel.User, "post", Route = "locations/{locationId}/events/{eventId}/users")] HttpRequest req,
            int locationId, int eventId, [SwaggerIgnore] ClaimsPrincipal user)
        {
            // if location was not found
            if (!await locationService.Exists(locationId))
                return new NotFoundObjectResult(new ErrorResponse(ErrorCode.LOCATION_NOT_FOUND));

            // if event was not found within location
            if (!await eventService.Exists(locationId, eventId))
                return new NotFoundObjectResult(new ErrorResponse(ErrorCode.EVENT_NOT_FOUND));

            // if user is the organiser
            int organiserId = await eventService.GetOrganiser(eventId);
            int userId = Int32.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier));
            if (organiserId == userId)
                return new BadRequestObjectResult(new ErrorResponse(ErrorCode.CANNOT_REGISTER_FOR_OWN_EVENT));

            // if user is already registered
            if (await eventService.RegistrationExists(eventId, userId))
                return new BadRequestObjectResult(new ErrorResponse(ErrorCode.EVENT_ALREADY_REGISTERED));

            // create registration
            bool isRegistered = await eventService.CreateRegistrationForEvent(eventId, userId);

            // if the registration was not created
            if (!isRegistered)
                return new BadRequestObjectResult(new ErrorResponse(ErrorCode.EVENT_REGISTRATION_LIMIT_REACHED));

            return new OkResult();
        }

        /// <summary>
        /// Delete a registration
        /// </summary>
        [FunctionName("LocationDeleteRegistration")]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> LocationDeleteRegistration(
            [HttpTrigger(AuthorizationLevel.User, "delete", Route = "locations/{locationId}/events/{eventId}/users")] HttpRequest req,
            int locationId, int eventId, [SwaggerIgnore] ClaimsPrincipal user)
        {
            // if location was not found
            if (!await locationService.Exists(locationId))
                return new NotFoundObjectResult(new ErrorResponse(ErrorCode.LOCATION_NOT_FOUND));

            // if event was not found within location
            if (!await eventService.Exists(locationId, eventId))
                return new NotFoundObjectResult(new ErrorResponse(ErrorCode.EVENT_NOT_FOUND));

            // delete registration
            int userId = Int32.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier));
            bool deletedRegistration = await eventService.DeleteRegistration(eventId, userId);

            // if the registration was not found
            if (!deletedRegistration)
                return new NotFoundObjectResult(new ErrorResponse(ErrorCode.EVENT_REGISTRATION_NOT_FOUND));

            return new OkResult();
        }
    }
}