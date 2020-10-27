using AzureFunctions.Extensions.Swashbuckle.Attribute;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using SkillsGardenApi.Models;
using SkillsGardenApi.Repositories;
using SkillsGardenApi.Services;
using SkillsGardenApi.Utils;
using SkillsGardenDTO;
using SkillsGardenDTO.Error;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SkillsGardenApi.Controllers
{
    class BeaconController
    {
        private BeaconService beaconService;
        private LocationService locationService;

        public BeaconController(BeaconService beaconService, LocationService locationService)
        {
            this.beaconService = beaconService;
            this.locationService = locationService;
        }

        [FunctionName("BeaconGetAll")]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> LocationGetAll(
            [HttpTrigger(AuthorizationLevel.User, "get", Route = "beacons")] HttpRequest req,
            [SwaggerIgnore] ClaimsPrincipal userClaim)

        {
            // only admin can update beacon
            if (!userClaim.IsInRole(UserType.Admin.ToString()))
                return ForbiddenObjectResult.Create(new ErrorResponse(ErrorCode.UNAUTHORIZED_ROLE_NO_PERMISSIONS));

            // get the locations
            List<Beacon> beacons = await beaconService.GetAllBeacons();

            return new OkObjectResult(beacons);
        }

        [FunctionName("BeaconGet")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> BeaconGet(
            [HttpTrigger(AuthorizationLevel.User, "get", Route = "beacons/{beaconId}")] HttpRequest req,
            int beaconId,
            [SwaggerIgnore] ClaimsPrincipal userClaim)
        {
            // only admin can update beacon
            if (!userClaim.IsInRole(UserType.Admin.ToString()))
                return new UnauthorizedObjectResult(new ErrorResponse(ErrorCode.UNAUTHORIZED_ROLE_NO_PERMISSIONS));

            // get the user
            Beacon beacon = await beaconService.GetBeaconById(beaconId);

            // if the user was not found
            if (beacon == null)
                return new NotFoundObjectResult(new ErrorResponse(ErrorCode.BEACON_NOT_FOUND));   //beacon not found

            return new OkObjectResult(beacon);
        }

        [FunctionName("BeaconCreate")]
        [ProducesResponseType(typeof(BeaconBody), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> BeaconCreate(
            [HttpTrigger(AuthorizationLevel.User, "post", Route = "beacons")]
            [RequestBodyType(typeof(BeaconBody), "The beacon to create")] HttpRequest req,
            [SwaggerIgnore] ClaimsPrincipal userClaim)
        {
            // only admin can update beacon
            if (!userClaim.IsInRole(UserType.Admin.ToString()))
                return ForbiddenObjectResult.Create(new ErrorResponse(ErrorCode.UNAUTHORIZED_ROLE_NO_PERMISSIONS));

            // deserialize request
            BeaconBody beaconBody;
            try
            {
                beaconBody = await SerializationUtil.Deserialize<BeaconBody>(req.Body);
            }
            catch (JsonException e)
            {
                return new BadRequestObjectResult(new ErrorResponse(400, e.Message));
            }

            // check for required fields
            if (beaconBody.Name == null) return new BadRequestObjectResult(new ErrorResponse(400, "Name is required"));
            if (beaconBody.LocationId == null) return new BadRequestObjectResult(new ErrorResponse(400, "LocationId is required"));
            if (beaconBody.Lat == null) return new BadRequestObjectResult(new ErrorResponse(400, "Latitude is required"));
            if (beaconBody.Lng == null) return new BadRequestObjectResult(new ErrorResponse(400, "Longitude is required"));

            if (!await locationService.Exists((int)beaconBody.LocationId))
                return new NotFoundObjectResult(new ErrorResponse(ErrorCode.LOCATION_NOT_FOUND));

            // create user
            Beacon createdBeacon = await this.beaconService.CreateBeacon(beaconBody);

            return new OkObjectResult(createdBeacon);
        }

        [FunctionName("BeaconUpdate")]
        [ProducesResponseType(typeof(UserBody), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> BeaconUpdate(
            [HttpTrigger(AuthorizationLevel.User, "put", Route = "beacons/{beaconId}")]
            [RequestBodyType(typeof(BeaconBody), "The beacon to update")] HttpRequest req,
            int beaconId,
            [SwaggerIgnore] ClaimsPrincipal userClaim)
        {
            // only admin can update beacon
            if (!userClaim.IsInRole(UserType.Admin.ToString()))
                return ForbiddenObjectResult.Create(new ErrorResponse(ErrorCode.UNAUTHORIZED_ROLE_NO_PERMISSIONS));

            // deserialize request
            BeaconBody beaconBody;
            try
            {
                beaconBody = await SerializationUtil.Deserialize<BeaconBody>(req.Body);
            }
            catch (JsonException e)
            {
                return new BadRequestObjectResult(new ErrorResponse(400, e.Message));
            }

            if (beaconBody.LocationId != null && !await locationService.Exists((int)beaconBody.LocationId))
                return new NotFoundObjectResult(new ErrorResponse(ErrorCode.LOCATION_NOT_FOUND));

            // update beacon
            Beacon updatedBeacon = await this.beaconService.UpdateBeacon(beaconId, beaconBody);

            // when beacon was not found
            if (updatedBeacon == null)
                return new NotFoundObjectResult(new ErrorResponse(ErrorCode.BEACON_NOT_FOUND));

            return new OkObjectResult(updatedBeacon);
        }

        [FunctionName("BeaconDelete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> BeaconDelete(
            [HttpTrigger(AuthorizationLevel.User, "delete", Route = "beacons/{beaconId}")] HttpRequest req,
            int beaconId,
            [SwaggerIgnore] ClaimsPrincipal userClaim)
        {
            // only admin can delete user
            if (!userClaim.IsInRole(UserType.Admin.ToString()))
                return ForbiddenObjectResult.Create(new ErrorResponse(ErrorCode.UNAUTHORIZED_TO_DELETE_USER));

            // delete beacon
            bool isDeleted = await beaconService.DeleteBeacon(beaconId);

            // if beacon was not found
            if (!isDeleted)
                return new NotFoundObjectResult(new ErrorResponse(ErrorCode.BEACON_NOT_FOUND));

            return new OkResult();
        }
    }
}
