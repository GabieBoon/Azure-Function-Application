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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SkillsGardenApi.Controllers
{
    public class ComponentController
    {
        private ComponentService componentService;
        private ExerciseService exerciseService;
        private LocationService locationService;

        public ComponentController(ComponentService componentService, ExerciseService exerciseService, LocationService locationService)
        {
            this.componentService = componentService;
            this.exerciseService = exerciseService;
            this.locationService = locationService;
        }

        [FunctionName("ComponentsGetAll")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ComponentsGetAll(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "locations/{locationId}/components")] HttpRequest req,
            int locationId)
        {
            // check if given location exists
            if (!await locationService.Exists(locationId))
                return new NotFoundObjectResult(new ErrorResponse(ErrorCode.LOCATION_NOT_FOUND));

            // get the components for this location
            List<Component> components = await componentService.GetComponentsByLocation(locationId);

            return new OkObjectResult(components);
        }

        [FunctionName("ComponentCreate")]
        [FormDataItem(Name = "Name", Description = "The name the component", Type = "string")]
        [FormDataItem(Name = "Description", Description = "The description of the component", Type = "string")]
        [FormDataItem(Name = "Image", Description = "The image of the component", Format = "binary", Type = "file")]
        [FormDataItem(Name = "Exercises", Description = "The exercises for the component", Type = "array")]
        [ProducesResponseType(typeof(ComponentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ComponentCreate(
            [HttpTrigger(AuthorizationLevel.User, "post", Route = "locations/{locationId}/components")] HttpRequest req,
            int locationId,
            [SwaggerIgnore] ClaimsPrincipal user)
        {
            // check if user has admin rights 
            if (!user.IsInRole(UserType.Admin.ToString()))
                return ForbiddenObjectResult.Create(new ErrorResponse(ErrorCode.UNAUTHORIZED_ROLE_NO_PERMISSIONS));

            // check if given location exists
            if (!await locationService.Exists(locationId))
                return new NotFoundObjectResult(new ErrorResponse(ErrorCode.LOCATION_NOT_FOUND));

            // get the form data
            IFormCollection formdata = await req.ReadFormAsync();
            ComponentBody componentBody;
            try {
                 componentBody = SerializationUtil.DeserializeFormData<ComponentBody>(formdata);
            } 
            catch(ValidationException e) {
                return new BadRequestObjectResult(new ErrorResponse(400, e.Message));
            }

            // check if all fields are filled in
            if (componentBody.Name == null || componentBody.Description == null || componentBody.Image == null || componentBody.Exercises == null)
                return new BadRequestObjectResult(new ErrorResponse(ErrorCode.INVALID_REQUEST_BODY));

            // check if given exercises exist
            foreach (int exerciseId in componentBody.Exercises)
            {
                if (!await exerciseService.Exists(exerciseId))
                    return new BadRequestObjectResult(new ErrorResponse(ErrorCode.INVALID_EXERCISE_PROVIDED));
            }

            // create new component
            int componentId = await componentService.CreateComponent(componentBody, locationId);

            // get the component
            ComponentResponse createdComponent = await componentService.GetComponent(locationId, componentId);

            return new OkObjectResult(createdComponent);
        }

        [FunctionName("ComponentsGetById")]
        [ProducesResponseType(typeof(ComponentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ComponentsGetById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "locations/{locationId}/components/{componentId}")] HttpRequest req,
            int locationId, int componentId)
        {
            // check if given location exists
            if (!await locationService.Exists(locationId))
                return new NotFoundObjectResult(new ErrorResponse(ErrorCode.LOCATION_NOT_FOUND));

            // get the component
            ComponentResponse component = await componentService.GetComponent(locationId, componentId);

            // check if given component exists
            if (component == null)
                return new NotFoundObjectResult(new ErrorResponse(ErrorCode.COMPONENT_NOT_FOUND));

            return new OkObjectResult(component);
        }
        
        [FunctionName("ComponentUpdateById")]
        [FormDataItem(Name = "Name", Description = "The name the component", Type = "string")]
        [FormDataItem(Name = "Description", Description = "The description of the component", Type = "string")]
        [FormDataItem(Name = "Image", Description = "The image of the component", Format = "binary", Type = "file")]
        [FormDataItem(Name = "Exercises", Description = "The exercises for the component", Type = "array")]
        [ProducesResponseType(typeof(ComponentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ComponentUpdateById(
            [HttpTrigger(AuthorizationLevel.User, "put", Route = "locations/{locationId}/components/{componentId}")] HttpRequest req,
            int locationId, int componentId,
            [SwaggerIgnore] ClaimsPrincipal user)
        {
            // only admin can delete component
            if (!user.IsInRole(UserType.Admin.ToString()))
                return ForbiddenObjectResult.Create(new ErrorResponse(ErrorCode.UNAUTHORIZED_ROLE_NO_PERMISSIONS));

            // check if given location exists
            if (!await locationService.Exists(locationId))
                return new NotFoundObjectResult(new ErrorResponse(ErrorCode.LOCATION_NOT_FOUND));

            // get the form data
            IFormCollection formdata = await req.ReadFormAsync();
            ComponentBody componentBody;
            try {
                componentBody = SerializationUtil.DeserializeFormData<ComponentBody>(formdata);
            }
            catch (ValidationException e) {
                return new BadRequestObjectResult(new ErrorResponse(400, e.Message));
            }

            // check if given exercises exist
            foreach (int exerciseId in componentBody.Exercises)
            {
                if (!await exerciseService.Exists(exerciseId))
                    return new BadRequestObjectResult(new ErrorResponse(ErrorCode.INVALID_EXERCISE_PROVIDED));
            }

            // update component
            Component updatedComponent = await componentService.UpdateComponent(componentBody, locationId, componentId);

            // if component was not found within location
            if (updatedComponent == null)
                return new NotFoundObjectResult(new ErrorResponse(ErrorCode.COMPONENT_NOT_FOUND));

            // get the updated component
            ComponentResponse response = await componentService.GetComponent(locationId, componentId);

            return new OkObjectResult(response);
        }

        [FunctionName("ComponentDelete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ComponentDelete(
            [HttpTrigger(AuthorizationLevel.User, "delete", Route = "locations/{locationId}/components/{componentId}")] HttpRequest req,
            int locationId, int componentId,
            [SwaggerIgnore] ClaimsPrincipal user)
        {
            // only admin can delete component
            if (!user.IsInRole(UserType.Admin.ToString()))
                return ForbiddenObjectResult.Create(new ErrorResponse(ErrorCode.UNAUTHORIZED_ROLE_NO_PERMISSIONS));

            // check if given location exists
            if (!await locationService.Exists(locationId))
                return new NotFoundObjectResult(new ErrorResponse(ErrorCode.LOCATION_NOT_FOUND));

            // check if given component exists
            if (!await componentService.Exists(locationId, componentId))
                return new NotFoundObjectResult(new ErrorResponse(ErrorCode.COMPONENT_NOT_FOUND));

            // delete component
            bool isDeleted = await componentService.DeleteComponent(componentId);

            if (!isDeleted)
                return new NotFoundObjectResult(new ErrorResponse(ErrorCode.COMPONENT_DELETE_FAILED));

            return new OkResult();
        }
    }
}
