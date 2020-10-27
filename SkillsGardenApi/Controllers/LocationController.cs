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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SkillsGardenApi.Controllers
{
    public class LocationController
    {
        private LocationService locationService;

        public LocationController(LocationService locationService)
        {
            this.locationService = locationService;
        }

        [FunctionName("LocationGetAll")]
        [ProducesResponseType(typeof(List<Location>), StatusCodes.Status200OK)]
        public async Task<IActionResult> LocationGetAll(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "locations")] HttpRequest req)
        {
            // get the locations
            List<Location> locations = await locationService.GetLocations();

            return new OkObjectResult(locations);
        }

        [FunctionName("LocationGet")]
        [ProducesResponseType(typeof(Location), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> LocationGet(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "locations/{locationId}")] HttpRequest req,
            int locationId)
        {
            // get the location
            Location location = await locationService.GetLocation(locationId);

            // check if requested location exists
            if (location == null)
                return new NotFoundObjectResult(new ErrorResponse(ErrorCode.LOCATION_NOT_FOUND));

            return new OkObjectResult(location);
        }

        [FunctionName("LocationCreate")]
        [FormDataItem(Name = "Name", Description = "The name the location", Type = "string")]
        [FormDataItem(Name = "City", Description = "The city of the location", Type = "string")]
        [FormDataItem(Name = "Lat", Description = "The lat of the location", Type = "double")]
        [FormDataItem(Name = "Lng", Description = "The lng of the location", Type = "double")]
        [FormDataItem(Name = "Image", Description = "The image of the location", Format = "binary", Type = "file")]
        [ProducesResponseType(typeof(Location), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> LocationCreate(
            [HttpTrigger(AuthorizationLevel.User, "post", Route = "locations")] HttpRequest req,
            [SwaggerIgnore] ClaimsPrincipal user)
        {
            // check if user has admin rights 
            if (!user.IsInRole(UserType.Admin.ToString()))
                return ForbiddenObjectResult.Create(new ErrorResponse(ErrorCode.UNAUTHORIZED_ROLE_NO_PERMISSIONS));

            // get the form data
            IFormCollection formdata = await req.ReadFormAsync();
            LocationBody locationBody;
            try {
                locationBody = SerializationUtil.DeserializeFormData<LocationBody>(formdata);
            }
            catch (ValidationException e) {
                return new BadRequestObjectResult(new ErrorResponse(400, e.Message));
            }

            // check if all fields are filled in
            if (locationBody.Name == null || locationBody.City == null || locationBody.Lat == null || locationBody.Lng == null || locationBody.Image == null)
                return new BadRequestObjectResult(new ErrorResponse(ErrorCode.INVALID_REQUEST_BODY));

            // create new location
            int locationId = await locationService.CreateLocation(locationBody);

            // get the created location
            Location createdLocation = await locationService.GetLocation(locationId);

            return new OkObjectResult(createdLocation);
        }

        /// <summary>
        /// Update location
        /// </summary>
        [FunctionName("LocationUpdate")]
        [FormDataItem(Name = "Name", Description = "The name the location", Type = "string")]
        [FormDataItem(Name = "City", Description = "The city of the location", Type = "string")]
        [FormDataItem(Name = "Lat", Description = "The lat of the location", Type = "double")]
        [FormDataItem(Name = "Lng", Description = "The lng of the location", Type = "double")]
        [FormDataItem(Name = "Image", Description = "The image of the location", Format = "binary", Type = "file")]
        [ProducesResponseType(typeof(Location), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> LocationUpdate(
            [HttpTrigger(AuthorizationLevel.User, "put", Route = "locations/{locationId}")] HttpRequest req,
            int locationId,
            [SwaggerIgnore] ClaimsPrincipal user)
        {
            // check if user has admin rights 
            if (!user.IsInRole(UserType.Admin.ToString()))
                return ForbiddenObjectResult.Create(new ErrorResponse(ErrorCode.UNAUTHORIZED_ROLE_NO_PERMISSIONS));

            // check if requested location exists
            if (!await locationService.Exists(locationId))
                return new NotFoundObjectResult(new ErrorResponse(ErrorCode.LOCATION_NOT_FOUND));

            // get the form data
            IFormCollection formdata = await req.ReadFormAsync();
            LocationBody locationBody;
            try {
                locationBody = SerializationUtil.DeserializeFormData<LocationBody>(formdata);
            }
            catch (ValidationException e) {
                return new BadRequestObjectResult(new ErrorResponse(400, e.Message));
            }

            // update location
            await locationService.UpdateLocation(locationBody, locationId); 

            // get the updated location
            Location updatedLocation = await locationService.GetLocation(locationId);

            return new OkObjectResult(updatedLocation);
        }

        /// <summary>
        /// Delete location
        /// </summary>
        [FunctionName("LocationDelete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> LocationDelete(
            [HttpTrigger(AuthorizationLevel.User, "delete", Route = "locations/{locationId}")] HttpRequest req,
            int locationId,
            [SwaggerIgnore] ClaimsPrincipal user)
        {
            // only admin can delete location
            if (!user.IsInRole(UserType.Admin.ToString()))
                return ForbiddenObjectResult.Create(new ErrorResponse(ErrorCode.UNAUTHORIZED_ROLE_NO_PERMISSIONS));

            // delete location
            bool isDeleted = await locationService.DeleteLocation(locationId);

            // if the location was not found
            if (!isDeleted)
                return new NotFoundObjectResult(new ErrorResponse(ErrorCode.LOCATION_NOT_FOUND));

            return new OkResult();
        }
    }
}