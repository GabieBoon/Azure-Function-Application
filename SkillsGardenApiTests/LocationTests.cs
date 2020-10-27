using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using SkillsGardenApi.Controllers;
using SkillsGardenApi.Models;
using SkillsGardenApi.Repositories;
using SkillsGardenApi.Services;
using SkillsGardenApiTests.Factory;
using SkillsGardenApiTests.Mock;
using SkillsGardenDTO.Error;
using System.Collections.Generic;
using System.Net.Http;
using Xunit;

namespace SkillsGardenApiTests
{
    public class LocationTests : UnitTest
    {
        private LocationRepository locationRepository;
        private LocationService locationService;
        private LocationController locationController;
        private IAzureService azureService;

        public LocationTests()
        {
            // create empty database
            this.locationRepository = new LocationRepository(CreateEmptyDatabase());
            this.azureService = new AzureServiceMock();
            this.locationService = new LocationService(this.locationRepository, this.azureService);

            // create location controller
            this.locationController = new LocationController(this.locationService);
        }

        //////////////////////////////////////////////    GET ALL //////////////////////////////////////////////
        
        [Fact]
        public async void GetAllLocationsTest()
        {
            await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(1));
            await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(2));
            await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(3));

            HttpRequest request = HttpRequestFactory.CreateGetRequest();

            ObjectResult result = (ObjectResult)await this.locationController.LocationGetAll(request);

            List<Location> locations = (List<Location>)result.Value;

            // status code should be 200 OK
            Assert.Equal(200, result.StatusCode);
            // amount of found locations should be 3
            Assert.Equal(3, locations.Count);
        }

        //////////////////////////////////////////////   GET SPECIFIC ID    //////////////////////////////////////////////

        [Fact]
        public async void GetSpecificLocationTest()
        {
            await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(10));

            HttpRequest request = HttpRequestFactory.CreateGetRequest();

            ObjectResult result = (ObjectResult)await this.locationController.LocationGet(request, 10);

            Location location = (Location)result.Value;

            // status code should be 200 OK
            Assert.Equal(200, result.StatusCode);
            // check if the right location was found
            Assert.Equal(10, location.Id);
        }

        //location not found
        [Fact]
        public async void GetSpecificLocationNotFoundTest()
        {
            await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(10));

            HttpRequest request = HttpRequestFactory.CreateGetRequest();

            ObjectResult result = (ObjectResult)await this.locationController.LocationGet(request, 5);

            ErrorResponse errorResponse = (ErrorResponse)result.Value;

            // status code should be 404 not found
            Assert.Equal(404, result.StatusCode);
            Assert.Equal(ErrorCode.LOCATION_NOT_FOUND, errorResponse.ErrorCodeEnum);

        }

        //////////////////////////////////////////////   CREATE NEW LOCATION   //////////////////////////////////////////////

        [Fact]
        public async void CreateNewLocationAsAdminTest()
        {
            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("Name", "Skillgarden Amsterdam");
            formdata.Add("City", "Amsterdam Centrum");
            formdata.Add("Lat", "1.2345235");
            formdata.Add("Lng", "1.2134234");

            HttpRequest requestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Post);

            ObjectResult result = (ObjectResult)await this.locationController.LocationCreate(requestmessage, this.adminClaim);

            // status code should be 200 OK
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async void CreateNewLocationAsNonAdminTest()
        {
            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("Name", "Skillgarden Amsterdam");
            formdata.Add("City", "Amsterdam Centrum");
            formdata.Add("Lat", "1.2345235");
            formdata.Add("Lng", "1.2134234");

            HttpRequest requestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Post);

            ObjectResult resultUser = (ObjectResult)await this.locationController.LocationCreate(requestmessage, this.userClaim);
            ObjectResult resultOrganiser = (ObjectResult)await this.locationController.LocationCreate(requestmessage, this.organiserClaim);

            ErrorResponse errorResponseUser = (ErrorResponse)resultUser.Value;
            ErrorResponse errorResponseOrganiser = (ErrorResponse)resultOrganiser.Value;


            // status code should be 403 FORBIDDEN
            Assert.Equal(403, resultUser.StatusCode);
            Assert.Equal(403, resultOrganiser.StatusCode);

            Assert.Equal(ErrorCode.UNAUTHORIZED_ROLE_NO_PERMISSIONS, errorResponseUser.ErrorCodeEnum);
            Assert.Equal(ErrorCode.UNAUTHORIZED_ROLE_NO_PERMISSIONS, errorResponseOrganiser.ErrorCodeEnum);

        }

        [Fact]
        public async void CreateNewLocationAsAdminNameTooLongTest()
        {
            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("Name", "Skillgarden Amsterdam op de grote buitenheuvel naast het postkantoor");
            formdata.Add("City", "Amsterdam Centrum");
            formdata.Add("Lat", "1.2345235");
            formdata.Add("Lng", "1.2134234");

            HttpRequest requestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Post);

            ObjectResult result = (ObjectResult)await this.locationController.LocationCreate(requestmessage, this.adminClaim);

            ErrorResponse errorResponse = (ErrorResponse)result.Value;

            // status code should be 400 and errormessage = "Name can not be longer than 50 characters"
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Name can not be longer than 50 characters", errorResponse.Message);
        }

        [Fact]
        public async void CreateNewLocationAsAdminNameTooShortTest()
        {
            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("Name", "X");
            formdata.Add("City", "Amsterdam Centrum");
            formdata.Add("Lat", "1.2345235");
            formdata.Add("Lng", "1.2134234");

            HttpRequest requestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Post);

            ObjectResult result = (ObjectResult)await this.locationController.LocationCreate(requestmessage, this.adminClaim);

            ErrorResponse errorResponse = (ErrorResponse)result.Value;

            // status code should be 400 and errormessage = "City must be at least 2 characters"
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Name must be at least 2 characters", errorResponse.Message);
        }

        [Fact]
        public async void CreateNewLocationAsAdminCityTooLongTest()
        {
            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("Name", "Skillgarden Wales");
            formdata.Add("City", "Llanfair­pwllgwyngyll­gogery­chwyrn­drobwll­llan­tysilio­gogo­goch");
            formdata.Add("Lat", "1.2345235");
            formdata.Add("Lng", "1.2134234");

            HttpRequest requestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Post);

            ObjectResult result = (ObjectResult)await this.locationController.LocationCreate(requestmessage, this.adminClaim);

            ErrorResponse errorResponse = (ErrorResponse)result.Value;

            // status code should be 400 and errorcode = "City can not be longer than 50 characters"
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("City can not be longer than 50 characters", errorResponse.Message);
        }

        [Fact]
        public async void CreateNewLocationAsAdminCityTooShortTest()
        {
            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("Name", "Skillgarden Wales");
            formdata.Add("City", "X");
            formdata.Add("Lat", "1.2345235");
            formdata.Add("Lng", "1.2134234");

            HttpRequest requestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Post);

            ObjectResult result = (ObjectResult)await this.locationController.LocationCreate(requestmessage, this.adminClaim);

            ErrorResponse errorResponse = (ErrorResponse)result.Value;

            // status code should be 400 and errorcode = "City must be at least 2 characters"
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("City must be at least 2 characters", errorResponse.Message);
        }

        [Fact]
        public async void CreateNewLocationAsAdminLatNoDoubleTest()
        {
            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("Name", "Skillgarden Amsterdam");
            formdata.Add("City", "Amsterdam");
            formdata.Add("Lat", "één punt twee drie negen vier");
            formdata.Add("Lng", "1.2134234");

            HttpRequest requestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Post);

            ObjectResult result = (ObjectResult)await this.locationController.LocationCreate(requestmessage, this.adminClaim);

            ErrorResponse errorResponse = (ErrorResponse)result.Value;

            // status code should be 400 and errorcode = "Lat must be a double"
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Lat must be a double", errorResponse.Message);
        }

        [Fact]
        public async void CreateNewLocationAsAdminLngNoDoubleTest()
        {
            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("Name", "Skillgarden Amsterdam");
            formdata.Add("City", "Amsterdam");
            formdata.Add("Lat", "1.2134234");
            formdata.Add("Lng", "één punt twee drie negen vier");

            HttpRequest requestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Post);

            ObjectResult result = (ObjectResult)await this.locationController.LocationCreate(requestmessage, this.adminClaim);

            ErrorResponse errorResponse = (ErrorResponse)result.Value;

            // status code should be 400 and errorcode = "Lat must be a double"
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Lng must be a double", errorResponse.Message);
        }

        [Fact]
        public async void CreateNewLocationAsAdminEmptyNameTest()
        {
            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("City", "Amsterdam");
            formdata.Add("Lat", "1.2134234");
            formdata.Add("Lng", "3.2341234");

            HttpRequest requestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Post);

            ObjectResult result = (ObjectResult)await this.locationController.LocationCreate(requestmessage, this.adminClaim);

            ErrorResponse errorResponse = (ErrorResponse)result.Value;

            // status code should be 400 INVALID_REQUEST_BODY
            Assert.Equal(400, result.StatusCode);
            Assert.Equal(ErrorCode.INVALID_REQUEST_BODY, errorResponse.ErrorCodeEnum);
        }

        [Fact]
        public async void CreateNewLocationAsAdminEmptyCityTest()
        {
            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("Name", "Skillgarden Amsterdam");
            formdata.Add("Lat", "1.2134234");
            formdata.Add("Lng", "2.1657865");

            HttpRequest requestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Post);

            ObjectResult result = (ObjectResult)await this.locationController.LocationCreate(requestmessage, this.adminClaim);

            ErrorResponse errorResponse = (ErrorResponse)result.Value;

            // status code should be 400 INVALID_REQUEST_BODY
            Assert.Equal(400, result.StatusCode);
            Assert.Equal(ErrorCode.INVALID_REQUEST_BODY, errorResponse.ErrorCodeEnum);
        }

        [Fact]
        public async void CreateNewLocationAsAdminEmptyLatTest()
        {
            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("City", "Amsterdam");
            formdata.Add("Name", "Skillgarden Amsterdam");
            formdata.Add("Lng", "1.1234123");

            HttpRequest requestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Post);

            ObjectResult result = (ObjectResult)await this.locationController.LocationCreate(requestmessage, this.adminClaim);

            ErrorResponse errorResponse = (ErrorResponse)result.Value;

            // status code should be 400 INVALID_REQUEST_BODY
            Assert.Equal(400, result.StatusCode);
            Assert.Equal(ErrorCode.INVALID_REQUEST_BODY, errorResponse.ErrorCodeEnum);
        }

        [Fact]
        public async void CreateNewLocationAsAdminEmptyLngTest()
        {
            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("City", "Amsterdam");
            formdata.Add("Name", "Skillgarden Amsterdam");
            formdata.Add("Lat", "1.2134234");

            HttpRequest requestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Post);

            ObjectResult result = (ObjectResult)await this.locationController.LocationCreate(requestmessage, this.adminClaim);

            ErrorResponse errorResponse = (ErrorResponse)result.Value;

            // status code should be 400 INVALID_REQUEST_BODY
            Assert.Equal(400, result.StatusCode);
            Assert.Equal(ErrorCode.INVALID_REQUEST_BODY, errorResponse.ErrorCodeEnum);
        }

        [Fact]
        public async void CreateNewLocationAsAdminNoImageTest()
        {
            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("Name", "Skillgarden Amsterdam");
            formdata.Add("City", "Amsterdam Centrum");
            formdata.Add("Lat", "1.2345235");
            formdata.Add("Lng", "1.2134234");

            HttpRequest requestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Post, "empty");

            ObjectResult result = (ObjectResult)await this.locationController.LocationCreate(requestmessage, this.adminClaim);

            ErrorResponse errorResponse = (ErrorResponse)result.Value;

            // status code should be 400 INVALID_REQUEST_BODY
            Assert.Equal(400, result.StatusCode);
            Assert.Equal(ErrorCode.INVALID_REQUEST_BODY, errorResponse.ErrorCodeEnum);
        }

        [Fact]
        public async void CreateNewLocationAsAdminUseGifAsImageTest()
        {
            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("Name", "Skillgarden Amsterdam");
            formdata.Add("City", "Amsterdam Centrum");
            formdata.Add("Lat", "1.2345235");
            formdata.Add("Lng", "1.2134234");

            HttpRequest requestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Post, "gif");

            ObjectResult result = (ObjectResult)await this.locationController.LocationCreate(requestmessage, this.adminClaim);

            ErrorResponse errorResponse = (ErrorResponse)result.Value;

            // status code should be 400
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async void CreateNewLocationAsAdminImageToBigTest()
        {
            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("Name", "Skillgarden Amsterdam");
            formdata.Add("City", "Amsterdam Centrum");
            formdata.Add("Lat", "1.2345235");
            formdata.Add("Lng", "1.2134234");

            HttpRequest requestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Post, "toBigImage");

            ObjectResult result = (ObjectResult)await this.locationController.LocationCreate(requestmessage, this.adminClaim);

            ErrorResponse errorResponse = (ErrorResponse)result.Value;

            // status code should be 400
            Assert.Equal(400, result.StatusCode);
        }


        [Fact]
        public async void CreateNewLocationAsAdminEmptyFormdataTest()
        {
            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            HttpRequest requestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Post, "empty");
            
            ObjectResult result = (ObjectResult)await this.locationController.LocationCreate(requestmessage, this.adminClaim);

            ErrorResponse errorResponse = (ErrorResponse)result.Value;

            // status code should be 400 INVALID_REQUEST_BODY
            Assert.Equal(400, result.StatusCode);
            Assert.Equal(ErrorCode.INVALID_REQUEST_BODY, errorResponse.ErrorCodeEnum);
        }

        //////////////////////////////////////////////   UPDATE LOCATION   //////////////////////////////////////////////

        [Fact]
        public async void UpdateLocationAsAdminTest()
        {
            // create user
            await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(5));

            // create updated user
            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("Name", "Skillgarden IJmuiden");
            formdata.Add("City", "IJmuiden Noord");
            formdata.Add("Lat", "1.3214");
            formdata.Add("Lng", "1.2343");

            // create put request
            HttpRequest putrequestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Put);

            ObjectResult result = (ObjectResult)await this.locationController.LocationUpdate(putrequestmessage, 5, this.adminClaim);

            Location resultLocation = (Location)result.Value;

            // status code should be 200 OK
            Assert.Equal(200, result.StatusCode);
            // the email should be updated
            Assert.Equal("Skillgarden IJmuiden", resultLocation.Name);
        }

        [Fact]
        public async void UpdateLocationAsNonAdminTest()
        {
            // create user
            await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(5));
            await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(10));

            // create updated user
            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("Name", "Skillgarden IJmuiden");
            formdata.Add("City", "IJmuiden Noord");
            formdata.Add("Lat", "1.3214");
            formdata.Add("Lng", "1.2343");

            // create put request
            HttpRequest putrequestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Put);

            ObjectResult resultUser = (ObjectResult)await this.locationController.LocationUpdate(putrequestmessage, 5, this.userClaim);
            ObjectResult resultOrganiser = (ObjectResult)await this.locationController.LocationUpdate(putrequestmessage, 10, this.organiserClaim);

            ErrorResponse errorMessageUser = (ErrorResponse)resultUser.Value;
            ErrorResponse errorMessageOrganiser = (ErrorResponse)resultOrganiser.Value;

            // status code should be 403 FORBIDDEN
            Assert.Equal(403, resultUser.StatusCode);
            Assert.Equal(403, resultOrganiser.StatusCode);

            Assert.Equal(ErrorCode.UNAUTHORIZED_ROLE_NO_PERMISSIONS, errorMessageUser.ErrorCodeEnum);
            Assert.Equal(ErrorCode.UNAUTHORIZED_ROLE_NO_PERMISSIONS, errorMessageOrganiser.ErrorCodeEnum);
        }

        [Fact]
        public async void UpdateLocationAsAdminNotFoundTest()
        {
            // create user
            await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(5));

            // create updated user
            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("Name", "Skillgarden Amsterdam op de grote buitenheuvel naast het postkantoor");
            formdata.Add("City", "IJmuiden Noord");
            formdata.Add("Lat", "1.3214");
            formdata.Add("Lng", "1.2343");

            // create put request
            HttpRequest putrequestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Put);

            ObjectResult result = (ObjectResult)await this.locationController.LocationUpdate(putrequestmessage, 10, this.adminClaim);

            ErrorResponse errorResponse = (ErrorResponse)result.Value;

            // status code should be 404 not found
            Assert.Equal(404, result.StatusCode);
            Assert.Equal(ErrorCode.LOCATION_NOT_FOUND, errorResponse.ErrorCodeEnum);
        }

        [Fact]
        public async void UpdateLocationAsAdminNameTooLongTest()
        {
            // create user
            await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(5));

            // create updated user
            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("Name", "Skillgarden Amsterdam op de grote buitenheuvel naast het postkantoor");
            formdata.Add("City", "IJmuiden Noord");
            formdata.Add("Lat", "1.3214");
            formdata.Add("Lng", "1.2343");

            // create put request
            HttpRequest putrequestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Put);

            ObjectResult result = (ObjectResult)await this.locationController.LocationUpdate(putrequestmessage, 5, this.adminClaim);

            ErrorResponse errorResponse = (ErrorResponse)result.Value;

            // status code should be 400 and errormessag = "Name can not be longer than 50 characters"
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Name can not be longer than 50 characters", errorResponse.Message);
        }

        [Fact]
        public async void UpdateLocationAsAdminNameTooShortTest()
        {
            // create user
            await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(5));

            // create updated user
            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("Name", "X");
            formdata.Add("City", "IJmuiden Noord");
            formdata.Add("Lat", "1.3214");
            formdata.Add("Lng", "1.2343");

            // create put request
            HttpRequest putrequestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Put);

            ObjectResult result = (ObjectResult)await this.locationController.LocationUpdate(putrequestmessage, 5, this.adminClaim);

            ErrorResponse errorResponse = (ErrorResponse)result.Value;

            // status code should be 400 and errormessag = "Name must be at least 2 characters"
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Name must be at least 2 characters", errorResponse.Message);
        }

        [Fact]
        public async void UpdateLocationAsAdminCityTooLongTest()
        {
            // create user
            await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(5));

            // create updated user
            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("Name", "Skillgarden Wales");
            formdata.Add("City", "Llanfair­pwllgwyngyll­gogery­chwyrn­drobwll­llan­tysilio­gogo­goch");
            formdata.Add("Lat", "1.3214");
            formdata.Add("Lng", "1.2343");

            // create put request
            HttpRequest putrequestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Put);

            ObjectResult result = (ObjectResult)await this.locationController.LocationUpdate(putrequestmessage, 5, this.adminClaim);

            ErrorResponse errorResponse = (ErrorResponse)result.Value;

            // status code should be 400 and errorcode = "City can not be longer than 50 characters"
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("City can not be longer than 50 characters", errorResponse.Message);
        }

        [Fact]
        public async void UpdateLocationAsAdminCityTooShortTest()
        {
            // create user
            await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(5));

            // create updated user
            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("Name", "Skillgarden Wales");
            formdata.Add("City", "X");
            formdata.Add("Lat", "1.3214");
            formdata.Add("Lng", "1.2343");

            // create put request
            HttpRequest putrequestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Put);

            ObjectResult result = (ObjectResult)await this.locationController.LocationUpdate(putrequestmessage, 5, this.adminClaim);

            ErrorResponse errorResponse = (ErrorResponse)result.Value;

            // status code should be 400 and errorcode = "City must be at least 2 characters"
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("City must be at least 2 characters", errorResponse.Message);
        }

        [Fact]
        public async void UpdateLocationAsAdminLatNoDoubleTest()
        {
            // create user
            await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(5));

            // create updated user
            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("Name", "Skillgarden Amsterdam");
            formdata.Add("City", "Amsterdam");
            formdata.Add("Lat", "één punt twee drie negen vier");
            formdata.Add("Lng", "1.2343");

            // create put request
            HttpRequest putrequestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Put);

            ObjectResult result = (ObjectResult)await this.locationController.LocationUpdate(putrequestmessage, 5, this.adminClaim);

            ErrorResponse errorResponse = (ErrorResponse)result.Value;

            // status code should be 400 INVALID_DOUBLEINPUT
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Lat must be a double", errorResponse.Message);
        }

        [Fact]
        public async void UpdateLocationAsAdminLngNoDoubleTest()
        {
            // create user
            await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(5));

            // create updated user
            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("Name", "Skillgarden Amsterdam");
            formdata.Add("City", "Amsterdam");
            formdata.Add("Lat", "1.2343");
            formdata.Add("Lng", "één punt twee drie negen vier");

            // create put request
            HttpRequest putrequestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Put);

            ObjectResult result = (ObjectResult)await this.locationController.LocationUpdate(putrequestmessage, 5, this.adminClaim);

            ErrorResponse errorResponse = (ErrorResponse)result.Value;

            // status code should be 400 INVALID_DOUBLEINPUT
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Lng must be a double", errorResponse.Message);
        }

        [Fact]
        public async void UpdateLocationAsAdminUseGifAsImageTest()
        {
            // create user
            await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(5));

            // create updated user
            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("Name", "Skillgarden Amsterdam");
            formdata.Add("City", "Amsterdam");
            formdata.Add("Lat", "1.2343");
            formdata.Add("Lng", "1.2343");

            // create put request
            HttpRequest putrequestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Put, "gif");

            ObjectResult result = (ObjectResult)await this.locationController.LocationUpdate(putrequestmessage, 5, this.adminClaim);

            ErrorResponse errorResponse = (ErrorResponse)result.Value;

            // status code should be 400
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async void UpdateLocationAsAdminImageToBigTest()
        {
            // create user
            await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(5));

            // create updated user
            Dictionary<string, StringValues> formdata = new Dictionary<string, StringValues>();

            formdata.Add("Name", "Skillgarden Amsterdam");
            formdata.Add("City", "Amsterdam");
            formdata.Add("Lat", "1.2343");
            formdata.Add("Lng", "1.2343");

            // create put request
            HttpRequest putrequestmessage = await HttpRequestFactory.CreateFormDataRequest(formdata, HttpMethod.Put, "toBigImage");

            ObjectResult result = (ObjectResult)await this.locationController.LocationUpdate(putrequestmessage, 5, this.adminClaim);

            ErrorResponse errorResponse = (ErrorResponse)result.Value;

            // status code should be 400
            Assert.Equal(400, result.StatusCode);
        }

        //////////////////////////////////////////////   DELETE LOCATION   //////////////////////////////////////////////

        [Fact]
        public async void DeleteLocationAsAdminTest()
        {
            // create location
            await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(9000));

            // create delete request
            HttpRequest deleteRequest = HttpRequestFactory.CreateDeleteRequest();

            OkResult result = (OkResult)await this.locationController.LocationDelete(deleteRequest, 9000, this.adminClaim);

            // status code should be 200 OK
            Assert.Equal(200, result.StatusCode);
            // the account should be removed
            Assert.Empty(await this.locationRepository.ListAsync());
        }

        [Fact]
        public async void DeleteLocationAsNonAdminTest()
        {
            // create location
            await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(9000));

            // create delete request
            HttpRequest deleteRequest = HttpRequestFactory.CreateDeleteRequest();

            ObjectResult resultUser = (ObjectResult)await this.locationController.LocationDelete(deleteRequest, 9000, this.userClaim);
            ObjectResult resultOrganiser = (ObjectResult)await this.locationController.LocationDelete(deleteRequest, 9000, this.organiserClaim);

            ErrorResponse errorMessageUser = (ErrorResponse)resultUser.Value;
            ErrorResponse errorMessageOrganiser = (ErrorResponse)resultOrganiser.Value;

            // status code should be 403 FORBIDDEN
            Assert.Equal(403, resultUser.StatusCode);
            Assert.Equal(403, resultOrganiser.StatusCode);

            Assert.Equal(ErrorCode.UNAUTHORIZED_ROLE_NO_PERMISSIONS, errorMessageUser.ErrorCodeEnum);
            Assert.Equal(ErrorCode.UNAUTHORIZED_ROLE_NO_PERMISSIONS, errorMessageOrganiser.ErrorCodeEnum);
        }

        [Fact]
        public async void DeleteLocationAsAdminNotFoundTest()
        {
            // create location
            await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(9000));

            // create delete request
            HttpRequest deleteRequest = HttpRequestFactory.CreateDeleteRequest();

            ObjectResult result = (ObjectResult)await this.locationController.LocationDelete(deleteRequest, 1, this.adminClaim);

            ErrorResponse errorResponse = (ErrorResponse)result.Value;

            // status code should be 404 not found
            Assert.Equal(404, result.StatusCode);
            Assert.Equal(ErrorCode.LOCATION_NOT_FOUND, errorResponse.ErrorCodeEnum);
        }
    }
}
