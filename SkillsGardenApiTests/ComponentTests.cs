using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using SkillsGardenApi.Controllers;
using SkillsGardenApi.Models;
using SkillsGardenApi.Repositories;
using SkillsGardenApi.Repositories.Context;
using SkillsGardenApi.Services;
using SkillsGardenApiTests.Factory;
using SkillsGardenApiTests.Mock;
using SkillsGardenDTO;
using SkillsGardenDTO.Error;
using SkillsGardenDTO.Response;
using System.Collections.Generic;
using System.Net.Http;
using Xunit;

namespace SkillsGardenApiTests
{
    public class ComponentTests : UnitTest
    {
        private ComponentRepository componentRepository;
        private ComponentController componentController;
        private ComponentService componentService;

        private LocationRepository locationRepository;
        private LocationService locationService;
        
        private ExerciseRepository exerciseRepository;
        private ExerciseService exerciseService;

        private IAzureService azureService;

        public ComponentTests()
        {
            DatabaseContext dbContext = CreateEmptyDatabase();

            this.locationRepository = new LocationRepository(dbContext);
            this.componentRepository = new ComponentRepository(dbContext);
            this.exerciseRepository = new ExerciseRepository(dbContext);

            this.azureService = new AzureServiceMock();
            this.locationService = new LocationService(this.locationRepository, this.azureService);
            this.componentService = new ComponentService(this.componentRepository, this.locationRepository, this.azureService);
            this.exerciseService = new ExerciseService(this.exerciseRepository, this.componentRepository);

            this.componentController = new ComponentController(this.componentService, this.exerciseService, this.locationService);
        }


        //////////////////////////////////////////////      GET ALL //////////////////////////////////////////////
        [Fact]
        public async void GetAllComponentsByLocationIdTest()
        {
            Location location1 = await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(1));
            Location location2 = await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(2));

            // create components for location 1
            await this.componentRepository.CreateAsync(ComponentFactory.CreateComponent(1, location1));
            await this.componentRepository.CreateAsync(ComponentFactory.CreateComponent(2, location1));
            await this.componentRepository.CreateAsync(ComponentFactory.CreateComponent(3, location1));

            //create components for location 2
            await this.componentRepository.CreateAsync(ComponentFactory.CreateComponent(4, location2));
            await this.componentRepository.CreateAsync(ComponentFactory.CreateComponent(5, location2));

            HttpRequest request = HttpRequestFactory.CreateGetRequest();

            //get all components location 1
            ObjectResult result = (ObjectResult)await this.componentController.ComponentsGetAll(request, 1);

            List<Component> components = (List<Component>)result.Value;

            // status code should be 200 OK
            Assert.Equal(200, result.StatusCode);
            // amount of found locations should be 3 for location 1
            Assert.Equal(3, components.Count);
        }

        //////////////////////////////////////////////   GET SPECIFIC ID    //////////////////////////////////////////////

        [Fact]
        public async void GetSpecificComponentTest()
        {
            Location location1 = await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(1));

            await this.componentRepository.CreateAsync(ComponentFactory.CreateComponent(1, location1));
            await this.componentRepository.CreateAsync(ComponentFactory.CreateComponent(2, location1));

            HttpRequest request = HttpRequestFactory.CreateGetRequest();

            ObjectResult result = (ObjectResult)await this.componentController.ComponentsGetById(request, 1, 1);

            ComponentResponse component = (ComponentResponse)result.Value;

            // status code should be 200 OK
            Assert.Equal(200, result.StatusCode);
            // check if the right location was found
            Assert.Equal(1, component.Id);
        }

        [Fact]
        public async void GetSpecificComponentNotFoundTest()
        {
            Location location1 = await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(1));

            await this.componentRepository.CreateAsync(ComponentFactory.CreateComponent(1, location1));

            HttpRequest request = HttpRequestFactory.CreateGetRequest();

            ObjectResult result = (ObjectResult)await this.componentController.ComponentsGetById(request, 1, 5);

            ErrorResponse errorResponse = (ErrorResponse)result.Value;

            // status code should be 200 OK
            Assert.Equal(404, result.StatusCode);
            // should return component not found errorresponse
            Assert.Equal(ErrorCode.COMPONENT_NOT_FOUND, errorResponse.ErrorCodeEnum);
        }

        //////////////////////////////////////////////   CREATE NEW COMPONENT   //////////////////////////////////////////////

        [Fact]
        public async void CreateNewComponentAsAdminTest()
        {
            //create location
            await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(1));

            //begin building new component
            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("Name", "Fiets parcour");
            formdata.Add("Description", "Race jij het snelst door de blauwe racebaan heen?");
            formdata.Add("Exercises", "1");
            AddExerciseToDatabase(1);

            HttpRequest requestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Post);

            ObjectResult result = (ObjectResult)await this.componentController.ComponentCreate(requestmessage, 1, this.adminClaim);

            // status code should be 200 OK
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async void CreateNewComponentAsNonAdminTest()
        {
            //create location
            await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(1));

            //begin building new component
            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("Name", "Stippentrap");
            formdata.Add("Description", "Leg jij behendig als een antilope het stippenparcour af?");
            formdata.Add("Exercises", "1");
            AddExerciseToDatabase(1);

            HttpRequest requestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Post);

            ObjectResult resultUser = (ObjectResult)await this.componentController.ComponentCreate(requestmessage, 1, this.userClaim);
            ObjectResult resultOrganiser = (ObjectResult)await this.componentController.ComponentCreate(requestmessage, 1, this.organiserClaim);

            ErrorResponse errorResponseUser = (ErrorResponse)resultUser.Value;
            ErrorResponse errorResponseOrganiser = (ErrorResponse)resultOrganiser.Value;

            // status code should be 403 FORBIDDEN
            Assert.Equal(403, resultUser.StatusCode);
            Assert.Equal(403, resultOrganiser.StatusCode);

            Assert.Equal(ErrorCode.UNAUTHORIZED_ROLE_NO_PERMISSIONS, errorResponseUser.ErrorCodeEnum);
            Assert.Equal(ErrorCode.UNAUTHORIZED_ROLE_NO_PERMISSIONS, errorResponseOrganiser.ErrorCodeEnum);
        }

        [Fact]
        public async void CreateNewComponentAsAdminNameTooLongTest()
        {
            //create location
            await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(1));

            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("Name", "Stippentrap gelegen naast de glijbaan in de skillgarden van Almere");
            formdata.Add("Description", "Leg jij behendig als een antilope het stippenparcour af?");
            formdata.Add("Exercises", "1");
            AddExerciseToDatabase(1);

            HttpRequest requestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Post);

            ObjectResult result = (ObjectResult)await this.componentController.ComponentCreate(requestmessage, 1, this.adminClaim);

            ErrorResponse errorResponse = (ErrorResponse)result.Value;

            // status code should be 400 "Name can not be longer than 50 characters"
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Name can not be longer than 50 characters", errorResponse.Message);
        }

        [Fact]
        public async void CreateNewComponentAsAdminNameTooShortTest()
        {
            //create location
            await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(1));

            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("Name", "X");
            formdata.Add("Description", "Leg jij behendig als een antilope het stippenparcour af?");
            formdata.Add("Exercises", "1");
            AddExerciseToDatabase(1);

            HttpRequest requestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Post);

            ObjectResult result = (ObjectResult)await this.componentController.ComponentCreate(requestmessage, 1, this.adminClaim);

            ErrorResponse errorResponse = (ErrorResponse)result.Value;

            // status code should be 400 "Name must be at least 2 characters"
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Name must be at least 2 characters", errorResponse.Message);
        }

        [Fact]
        public async void CreateNewComponentAsAdminDescriptionTooLongTest()
        {
            //create location
            await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(1));

            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("Name", "Stippentrap");
            formdata.Add("Description", "Stippentrap gelegen naast de glijbaan in de skillgarden van Almere. Stippentrap gelegen naast de glijbaan in de skillgarden van Almere. Stippentrap gelegen naast de glijbaan in de skillgarden van Almere. Stippentrap gelegen naast de glijbaan in de skillgarden van Almere. Stippentrap gelegen naast de glijbaan in de skillgarden van Almere. Stippentrap gelegen naast de glijbaan in de skillgarden van Almere. Stippentrap gelegen naast de glijbaan in de skillgarden van Almere. Stippentrap gelegen naast de glijbaan in de skillgarden van Almere. ");
            formdata.Add("Exercises", "1");
            AddExerciseToDatabase(1);

            HttpRequest requestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Post);

            ObjectResult result = (ObjectResult)await this.componentController.ComponentCreate(requestmessage, 1, this.adminClaim);

            ErrorResponse errorResponse = (ErrorResponse)result.Value;

            // status code should be 400 "Description can not be longer than 500 characters"
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Description can not be longer than 500 characters", errorResponse.Message);
        }

        [Fact]
        public async void CreateNewComponentAsAdminDescriptionTooShortTest()
        {
            //create location
            await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(1));

            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("Name", "Stippentrap");
            formdata.Add("Description", "X");
            formdata.Add("Exercises", "1");
            AddExerciseToDatabase(1);

            HttpRequest requestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Post);

            ObjectResult result = (ObjectResult)await this.componentController.ComponentCreate(requestmessage, 1, this.adminClaim);

            ErrorResponse errorResponse = (ErrorResponse)result.Value;

            // status code should be 400 "Description must be at least 2 characters"
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Description must be at least 2 characters", errorResponse.Message);
        }

        [Fact]
        public async void CreateNewComponentAsAdminNoNameTest()
        {
            //create location
            await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(1));

            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("Description", "Leg jij behendig als een antilope het stippenparcour af?");
            formdata.Add("Exercises", "1");
            AddExerciseToDatabase(1);

            HttpRequest requestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Post);

            ObjectResult result = (ObjectResult)await this.componentController.ComponentCreate(requestmessage, 1, this.adminClaim);

            ErrorResponse errorResponse = (ErrorResponse)result.Value;

            // status code should be 400 INVALID_REQUEST_BODY
            Assert.Equal(400, result.StatusCode);
            Assert.Equal(ErrorCode.INVALID_REQUEST_BODY, errorResponse.ErrorCodeEnum);
        }

        [Fact]
        public async void CreateNewComponentAsAdminNoDescriptionTest()
        {
            //create location
            await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(1));

            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("Name", "Stippentrap");
            formdata.Add("Exercises", "1");
            AddExerciseToDatabase(1);

            HttpRequest requestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Post);

            ObjectResult result = (ObjectResult)await this.componentController.ComponentCreate(requestmessage, 1, this.adminClaim);

            ErrorResponse errorResponse = (ErrorResponse)result.Value;

            // status code should be 400 INVALID_REQUEST_BODY
            Assert.Equal(400, result.StatusCode);
            Assert.Equal(ErrorCode.INVALID_REQUEST_BODY, errorResponse.ErrorCodeEnum);
        }

        [Fact]
        public async void CreateNewComponentAsAdminNoExercisesTest()
        {
            //create location
            await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(1));

            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("Name", "Stippentrap");
            formdata.Add("Description", "Leg jij behendig als een antilope het stippenparcour af?");

            HttpRequest requestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Post, "empty");

            ObjectResult result = (ObjectResult)await this.componentController.ComponentCreate(requestmessage, 1, this.adminClaim);

            ErrorResponse errorResponse = (ErrorResponse)result.Value;

            // status code should be 400 INVALID_REQUEST_BODY
            Assert.Equal(400, result.StatusCode);
            Assert.Equal(ErrorCode.INVALID_REQUEST_BODY, errorResponse.ErrorCodeEnum);
        }

        [Fact]
        public async void CreateNewComponentAsAdminInvalidExercisesTest()
        {
            //create location
            await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(1));

            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("Name", "Stippentrap");
            formdata.Add("Description", "Leg jij behendig als een antilope het stippenparcour af?");
            formdata.Add("Exercises", "invalid");

            HttpRequest requestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Post, "empty");

            ObjectResult result = (ObjectResult)await this.componentController.ComponentCreate(requestmessage, 1, this.adminClaim);

            ErrorResponse errorResponse = (ErrorResponse)result.Value;

            // status code should be 400 INVALID_REQUEST_BODY
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Exercises must be an array of integers", errorResponse.Message);
        }

        [Fact]
        public async void CreateNewComponentAsAdminNoImageTest()
        {
            //create location
            await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(1));

            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("Name", "Stippentrap");
            formdata.Add("Description", "Leg jij behendig als een antilope het stippenparcour af?");
            formdata.Add("Exercises", "1");
            AddExerciseToDatabase(1);

            HttpRequest requestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Post, "empty");

            ObjectResult result = (ObjectResult)await this.componentController.ComponentCreate(requestmessage, 1, this.adminClaim);

            ErrorResponse errorResponse = (ErrorResponse)result.Value;

            // status code should be 400 INVALID_REQUEST_BODY
            Assert.Equal(400, result.StatusCode);
            Assert.Equal(ErrorCode.INVALID_REQUEST_BODY, errorResponse.ErrorCodeEnum);
        }

        [Fact]
        public async void CreateNewComponentAsAdminUseGifAsImageTest()
        {
            //create location
            await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(1));

            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("Name", "Stippentrap");
            formdata.Add("Description", "Leg jij behendig als een antilope het stippenparcour af?");
            formdata.Add("Exercises", "1");
            AddExerciseToDatabase(1);

            HttpRequest requestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Post, "gif");

            ObjectResult result = (ObjectResult)await this.componentController.ComponentCreate(requestmessage, 1, this.adminClaim);

            // status code should be 400
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async void CreateNewComponentAsAdminImageToBigTest()
        {
            //create location
            await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(1));

            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("Name", "Stippentrap");
            formdata.Add("Description", "Leg jij behendig als een antilope het stippenparcour af?");
            formdata.Add("Exercises", "1");
            AddExerciseToDatabase(1);

            HttpRequest requestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Post, "toBigImage");

            ObjectResult result = (ObjectResult)await this.componentController.ComponentCreate(requestmessage, 1, this.adminClaim);

            // status code should be 400
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async void CreateNewComponentAsAdminEmptyFormdataTest()
        {
            //create location
            await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(1));

            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            HttpRequest requestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Post, "empty");

            ObjectResult result = (ObjectResult)await this.componentController.ComponentCreate(requestmessage, 1, this.adminClaim);

            ErrorResponse errorResponse = (ErrorResponse)result.Value;

            // status code should be 400 INVALID_REQUEST_BODY
            Assert.Equal(400, result.StatusCode);
            Assert.Equal(ErrorCode.INVALID_REQUEST_BODY, errorResponse.ErrorCodeEnum);
        }

        //////////////////////////////////////////////   UPDATE COMPONENT   //////////////////////////////////////////////

        [Fact]
        public async void UpdateComponentAsAdminTest()
        {
            //create location and location
            Location location = await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(1));
            await this.componentRepository.CreateAsync(ComponentFactory.CreateComponent(1, location));

            //begin building new component
            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("Name", "Fiets parcour");
            formdata.Add("Description", "Race jij het snelst door de blauwe racebaan heen?");
            formdata.Add("Exercises", "1");
            AddExerciseToDatabase(1);

            HttpRequest requestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Put);

            ObjectResult result = (ObjectResult)await this.componentController.ComponentUpdateById(requestmessage, 1, 1, this.adminClaim);

            ComponentResponse resultComponent = (ComponentResponse)result.Value;

            // status code should be 200 OK
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Fiets parcour", resultComponent.Name);
        }

        [Fact]
        public async void UpdateComponentAsNonAdminTest()
        {
            //create location and location
            Location location = await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(1));
            await this.componentRepository.CreateAsync(ComponentFactory.CreateComponent(1, location));

            //begin building new component
            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("Name", "Fiets parcour");
            formdata.Add("Description", "Race jij het snelst door de blauwe racebaan heen?");
            formdata.Add("Exercises", "1");
            AddExerciseToDatabase(1);

            HttpRequest requestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Put);

            ObjectResult resultUser = (ObjectResult)await this.componentController.ComponentUpdateById(requestmessage, 1, 1, this.userClaim);
            ObjectResult resultOrganiser = (ObjectResult)await this.componentController.ComponentUpdateById(requestmessage, 1, 1, this.organiserClaim);

            ErrorResponse errorMessageUser = (ErrorResponse)resultUser.Value;
            ErrorResponse errorMessageOrganiser = (ErrorResponse)resultOrganiser.Value;

            // status code should be 403 FORBIDDEN
            Assert.Equal(403, resultUser.StatusCode);
            Assert.Equal(403, resultOrganiser.StatusCode);

            Assert.Equal(ErrorCode.UNAUTHORIZED_ROLE_NO_PERMISSIONS, errorMessageUser.ErrorCodeEnum);
            Assert.Equal(ErrorCode.UNAUTHORIZED_ROLE_NO_PERMISSIONS, errorMessageOrganiser.ErrorCodeEnum);
        }

        [Fact]
        public async void UpdateComponentAsAdminComponentNotFoundTest()
        {
            //create location and location
            Location location = await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(1));
            await this.componentRepository.CreateAsync(ComponentFactory.CreateComponent(1, location));

            //begin building new component
            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("Name", "Stippentrap");
            formdata.Add("Description", "Leg jij behendig als een antilope het stippenparcour af?");
            formdata.Add("Exercises", "1");
            AddExerciseToDatabase(1);

            HttpRequest requestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Put);

            ObjectResult result = (ObjectResult)await this.componentController.ComponentUpdateById(requestmessage, 1, 5, this.adminClaim);

            ErrorResponse errorResponse = (ErrorResponse)result.Value;

            // status code should be 404 not found
            Assert.Equal(404, result.StatusCode);
            Assert.Equal(ErrorCode.COMPONENT_NOT_FOUND, errorResponse.ErrorCodeEnum);
        }

        [Fact]
        public async void UpdateComponentAsAdminNameTooLongTest()
        {
            //create location and location
            Location location = await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(1));
            await this.componentRepository.CreateAsync(ComponentFactory.CreateComponent(1, location));

            //begin building new component
            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("Name", "Stippentrap gelegen naast de glijbaan in de skillgarden van Almere");
            formdata.Add("Description", "Leg jij behendig als een antilope het stippenparcour af?");
            formdata.Add("Exercises", "1");
            AddExerciseToDatabase(1);

            HttpRequest requestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Put);

            ObjectResult result = (ObjectResult)await this.componentController.ComponentUpdateById(requestmessage, 1, 1, this.adminClaim);

            ErrorResponse errorResponse = (ErrorResponse)result.Value;

            // status code should be 401 UNAUTHORIZED
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Name can not be longer than 50 characters", errorResponse.Message);
        }

        [Fact]
        public async void UpdateComponentAsAdminNameTooShortTest()
        {
            //create location and location
            Location location = await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(1));
            await this.componentRepository.CreateAsync(ComponentFactory.CreateComponent(1, location));

            //begin building new component
            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("Name", "X");
            formdata.Add("Description", "Leg jij behendig als een antilope het stippenparcour af?");
            formdata.Add("Exercises", "1");
            AddExerciseToDatabase(1);

            HttpRequest requestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Put);

            ObjectResult result = (ObjectResult)await this.componentController.ComponentUpdateById(requestmessage, 1, 1, this.adminClaim);

            ErrorResponse errorResponse = (ErrorResponse)result.Value;

            // status code should be 400 "Name must be at least 2 characters"
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Name must be at least 2 characters", errorResponse.Message);
        }

        [Fact]
        public async void UpdateComponentAsAdminDescriptionTooLongTest()
        {
            //create location and location
            Location location = await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(1));
            await this.componentRepository.CreateAsync(ComponentFactory.CreateComponent(1, location));

            //begin building new component
            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("Name", "Stippentrap");
            formdata.Add("Description", "Stippentrap gelegen naast de glijbaan in de skillgarden van Almere. Stippentrap gelegen naast de glijbaan in de skillgarden van Almere. Stippentrap gelegen naast de glijbaan in de skillgarden van Almere. Stippentrap gelegen naast de glijbaan in de skillgarden van Almere. Stippentrap gelegen naast de glijbaan in de skillgarden van Almere. Stippentrap gelegen naast de glijbaan in de skillgarden van Almere. Stippentrap gelegen naast de glijbaan in de skillgarden van Almere. Stippentrap gelegen naast de glijbaan in de skillgarden van Almere. ");
            formdata.Add("Exercises", "1");
            AddExerciseToDatabase(1);

            HttpRequest requestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Put);

            ObjectResult result = (ObjectResult)await this.componentController.ComponentUpdateById(requestmessage, 1, 1, this.adminClaim);

            ErrorResponse errorResponse = (ErrorResponse)result.Value;

            // status code should be 400 "Description can not be longer than 500 characters"
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Description can not be longer than 500 characters", errorResponse.Message);
        }

        [Fact]
        public async void UpdateComponentAsAdminDescriptionTooShortTest()
        {
            //create location and location
            Location location = await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(1));
            await this.componentRepository.CreateAsync(ComponentFactory.CreateComponent(1, location));

            //begin building new component
            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("Name", "Stippentrap");
            formdata.Add("Description", "X");
            formdata.Add("Exercises", "1");
            AddExerciseToDatabase(1);

            HttpRequest requestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Put);

            ObjectResult result = (ObjectResult)await this.componentController.ComponentUpdateById(requestmessage, 1, 1, this.adminClaim);

            ErrorResponse errorResponse = (ErrorResponse)result.Value;

            // status code should be 400 "Description must be at least 2 characters"
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Description must be at least 2 characters", errorResponse.Message);
        }

        [Fact]
        public async void UpdateComponentAsAdminUseGifAsImageTest()
        {
            //create location and location
            Location location = await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(1));
            await this.componentRepository.CreateAsync(ComponentFactory.CreateComponent(1, location));

            //begin building new component
            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("Name", "Stippentrap");
            formdata.Add("Description", "Leg jij behendig als een antilope het stippenparcour af?");
            formdata.Add("Exercises", "1");
            AddExerciseToDatabase(1);

            HttpRequest requestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Put, "gif");

            ObjectResult result = (ObjectResult)await this.componentController.ComponentUpdateById(requestmessage, 1, 1, this.adminClaim);

            ErrorResponse errorResponse = (ErrorResponse)result.Value;

            // status code should be 400
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async void UpdateComponentAsAdminImageToBigTest()
        {
            //create location and location
            Location location = await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(1));
            await this.componentRepository.CreateAsync(ComponentFactory.CreateComponent(1, location));

            //begin building new component
            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("Name", "Stippentrap");
            formdata.Add("Description", "Leg jij behendig als een antilope het stippenparcour af?");
            formdata.Add("Exercises", "1");
            AddExerciseToDatabase(1);

            HttpRequest requestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Put, "toBigImage");

            ObjectResult result = (ObjectResult)await this.componentController.ComponentUpdateById(requestmessage, 1, 1, this.adminClaim);

            // status code should be 400
            Assert.Equal(400, result.StatusCode);
        }

        //////////////////////////////////////////////   DELETE COMPONENT   //////////////////////////////////////////////

        [Fact]
        public async void DeleteComponentAsAdminTest()
        {
            //create location and component
            Location location = await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(1));
            await this.componentRepository.CreateAsync(ComponentFactory.CreateComponent(1, location));

            // create delete request
            HttpRequest deleteRequest = HttpRequestFactory.CreateDeleteRequest();

            OkResult result = (OkResult)await this.componentController.ComponentDelete(deleteRequest, 1, 1, this.adminClaim);

            // status code should be 200 OK
            Assert.Equal(200, result.StatusCode);
            // the account should be removed
            Assert.Empty(await this.componentRepository.ListAsync());
        }

        [Fact]
        public async void DeleteComponentAsNonAdminTest()
        {
            //create location and component
            Location location = await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(1));
            await this.componentRepository.CreateAsync(ComponentFactory.CreateComponent(1, location));

            // create delete request
            HttpRequest deleteRequest = HttpRequestFactory.CreateDeleteRequest();

            ObjectResult resultUser = (ObjectResult)await this.componentController.ComponentDelete(deleteRequest, 1, 1, this.userClaim);
            ObjectResult resultOrganiser = (ObjectResult)await this.componentController.ComponentDelete(deleteRequest, 1, 1, this.organiserClaim);

            ErrorResponse errorMessageUser = (ErrorResponse)resultUser.Value;
            ErrorResponse errorMessageOrganiser = (ErrorResponse)resultOrganiser.Value;

            // status code should be 403 FORBIDDEN
            Assert.Equal(403, resultUser.StatusCode);
            Assert.Equal(403, resultOrganiser.StatusCode);

            Assert.Equal(ErrorCode.UNAUTHORIZED_ROLE_NO_PERMISSIONS, errorMessageUser.ErrorCodeEnum);
            Assert.Equal(ErrorCode.UNAUTHORIZED_ROLE_NO_PERMISSIONS, errorMessageOrganiser.ErrorCodeEnum);
        }

        [Fact]
        public async void DeleteComponentAsAdminNotFoundTest()
        {
            //create location and component
            Location location = await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(1));
            await this.componentRepository.CreateAsync(ComponentFactory.CreateComponent(1, location));

            // create delete request
            HttpRequest deleteRequest = HttpRequestFactory.CreateDeleteRequest();

            ObjectResult result = (ObjectResult)await this.componentController.ComponentDelete(deleteRequest, 1, 5, this.adminClaim);

            ErrorResponse errorResponse = (ErrorResponse)result.Value;

            // status code should be 404 not found
            Assert.Equal(404, result.StatusCode);
            Assert.Equal(ErrorCode.COMPONENT_NOT_FOUND, errorResponse.ErrorCodeEnum);
        }

        private async void AddExerciseToDatabase(int id)
        {
            Exercise exercise1 = ExerciseFactory.CreateExercise("Touwtje springen");
            exercise1.Id = id;
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
        }
    }
}