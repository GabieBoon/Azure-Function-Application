using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
using System;
using System.Collections.Generic;
using System.Security.Claims;
using Xunit;

namespace SkillsGardenApiTests
{
    public class UserTests : UnitTest
    {
        private UserRepository userRepository;
        private UserService userService;
        private UserController userController;

        private BeaconRepository beaconRepository;
        private BeaconService beaconService;

        private EventRepository eventRepository;
        private EventService eventService;

        private IAzureService azureService;

        public UserTests()
        {
            // create database
            DatabaseContext dbContext = CreateEmptyDatabase();

            this.azureService = new AzureServiceMock();    

            this.userRepository = new UserRepository(dbContext);
            this.userService = new UserService(this.userRepository);

            this.beaconRepository = new BeaconRepository(dbContext);
            this.beaconService = new BeaconService(this.beaconRepository);

            this.eventRepository = new EventRepository(dbContext);
            this.eventService = new EventService(this.eventRepository, this.userRepository, this.azureService);

            // create user controller
            this.userController = new UserController(this.userService, this.beaconService, this.eventService);
        }

        [Fact]
        public void AlwaysPass()
        {
            int x = 1;

            Assert.Equal(1, x);
        }

        [Fact]
        public async void GetAllUsersAsAdminTest()
        {
            // add 3 different users to database
            await this.userRepository.CreateAsync(UserFactory.CreateNormalUser());
            await this.userRepository.CreateAsync(UserFactory.CreateOrganiserUser());
            await this.userRepository.CreateAsync(UserFactory.CreateAdminUser());

            HttpRequest request = HttpRequestFactory.CreateGetRequest();

            ObjectResult result = (ObjectResult)await this.userController.UsersGetAll(request, this.adminClaim);

            List<UserResponse> users = (List<UserResponse>)result.Value;

            // status code should be 200 OK
            Assert.Equal(200, result.StatusCode);
            // amount of found users should be 3
            Assert.Equal(3, users.Count);
            // check the account type of the users
            Assert.True(users[0].Type == UserType.User);
            Assert.True(users[1].Type == UserType.Organiser);
            Assert.True(users[2].Type == UserType.Admin);
        }

        [Fact]
        public async void GetAllUsersAsNonAdminTest()
        {
            // add user to database
            await this.userRepository.CreateAsync(UserFactory.CreateNormalUser());

            HttpRequest request = HttpRequestFactory.CreateGetRequest();

            ObjectResult resultUser = (ObjectResult)await this.userController.UsersGetAll(request, this.organiserClaim);
            ObjectResult resultOrganiser = (ObjectResult)await this.userController.UsersGetAll(request, this.organiserClaim);

            ErrorResponse errorMessageUser = (ErrorResponse)resultUser.Value;
            ErrorResponse errorMessageOrganiser = (ErrorResponse)resultOrganiser.Value;

            // status code should be 403 FORBIDDEN
            Assert.Equal(403, resultUser.StatusCode);
            Assert.Equal(403, resultOrganiser.StatusCode);

            // error code must be unauthorized because role has no permissions
            Assert.Equal(ErrorCode.UNAUTHORIZED_ROLE_NO_PERMISSIONS, errorMessageUser.ErrorCodeEnum);
            Assert.Equal(ErrorCode.UNAUTHORIZED_ROLE_NO_PERMISSIONS, errorMessageOrganiser.ErrorCodeEnum);
        }

        [Fact]
        public async void GetOneUserAsAdminTest()
        {
            // add user to database
            await this.userRepository.CreateAsync(UserFactory.CreateNormalUser(1000));

            HttpRequest request = HttpRequestFactory.CreateGetRequest();

            ObjectResult result = (ObjectResult)await this.userController.UsersGet(request, 1000, this.adminClaim);

            UserResponse user = (UserResponse)result.Value;

            // status code should be 200 OK
            Assert.Equal(200, result.StatusCode);
            // check if the right user was found
            Assert.Equal(1000, user.Id);
        }

        [Fact]
        public async void GetOneUserAsUserTest()
        {
            // add user to database
            await this.userRepository.CreateAsync(UserFactory.CreateNormalUser(4));
            await this.userRepository.CreateAsync(UserFactory.CreateNormalUser(1000));

            HttpRequest request = HttpRequestFactory.CreateGetRequest();

            ObjectResult resultAuthorized = (ObjectResult)await this.userController.UsersGet(request, 4, this.userClaim);
            ObjectResult resultNotAuthorized = (ObjectResult)await this.userController.UsersGet(request, 1000, this.userClaim);

            UserResponse user = (UserResponse)resultAuthorized.Value;

            // status code should be 200 OK
            Assert.Equal(200, resultAuthorized.StatusCode);
            // status code should be 403 FORBIDDEN
            Assert.Equal(403, resultNotAuthorized.StatusCode);
            // check if the right user was found
            Assert.Equal(4, user.Id);
        }

        [Fact]
        public async void GetOneUserAsAdminNotFoundTest()
        {
            HttpRequest request = HttpRequestFactory.CreateGetRequest();

            ObjectResult result = (ObjectResult)await this.userController.UsersGet(request, 1000, this.adminClaim);

            ErrorResponse errorResponse = (ErrorResponse)result.Value;

            // status code should be 200 OK
            Assert.Equal(404, result.StatusCode);
            // check if error response is user not found
            Assert.Equal(ErrorCode.USER_NOT_FOUND, errorResponse.ErrorCodeEnum);
        }

        [Fact]
        public async void CreateNewUsersAsAdminTest()
        {
            HttpRequest request = HttpRequestFactory.CreatePostRequest<User>(UserFactory.CreateNormalUser());

            ObjectResult result = (ObjectResult)await this.userController.UserCreate(request, this.adminClaim);

            // status code should be 200 OK
            Assert.Equal(200, result.StatusCode);
            // database should contain 1 user
            Assert.Single(await this.userRepository.ListAsync());
        }

        [Fact]
        public async void CreateNewUsersAsNonAdminTest()
        {
            HttpRequest requestAnonymous = HttpRequestFactory.CreatePostRequest<User>(UserFactory.CreateNormalUser(1, "test1@mail.com"));
            HttpRequest requestUser = HttpRequestFactory.CreatePostRequest<User>(UserFactory.CreateNormalUser(2, "test2@mail.com"));
            HttpRequest requestOrganiser = HttpRequestFactory.CreatePostRequest<User>(UserFactory.CreateNormalUser(3, "test3@mail.com"));
            HttpRequest requestWithType = HttpRequestFactory.CreatePostRequest<User>(UserFactory.CreateOrganiserUser());

            ObjectResult resultAnonymous = (ObjectResult)await this.userController.UserCreate(requestAnonymous, new ClaimsPrincipal());
            ObjectResult resultUser = (ObjectResult)await this.userController.UserCreate(requestUser, this.userClaim);
            ObjectResult resultOrganiser = (ObjectResult)await this.userController.UserCreate(requestOrganiser, this.organiserClaim);
            ObjectResult resultUserWithType = (ObjectResult)await this.userController.UserCreate(requestWithType, this.userClaim);

            UserResponse resultUserAnonymous = (UserResponse)resultAnonymous.Value;
            UserResponse resultUserUser = (UserResponse)resultUser.Value;
            UserResponse resultUserOrganiser = (UserResponse)resultOrganiser.Value;
            ErrorResponse errorMessageUserWithType = (ErrorResponse)resultUserWithType.Value;

            // status code should be 200 OK
            Assert.Equal(200, resultAnonymous.StatusCode);
            Assert.Equal(200, resultUser.StatusCode);
            Assert.Equal(200, resultOrganiser.StatusCode);
            // status code should be 403 FORBIDDEN
            Assert.Equal(403, resultUserWithType.StatusCode);

            // returned users should not be null
            Assert.NotNull(resultUserAnonymous);
            Assert.NotNull(resultUserUser);
            Assert.NotNull(resultUserOrganiser);
            // error code must be unauthorized because role has no permissions
            Assert.Equal(ErrorCode.UNAUTHORIZED_TO_SET_USER_TYPE, errorMessageUserWithType.ErrorCodeEnum);
            // database should contain 3 users
            Assert.Equal(3, (await this.userRepository.ListAsync()).Count);
        }

        [Fact]
        public async void UpdateUserAsNonAdminTest()
        {
            // create users
            await this.userRepository.CreateAsync(UserFactory.CreateNormalUser(4, "old1@mail.com"));
            await this.userRepository.CreateAsync(UserFactory.CreateNormalUser(2000, "old2@mail.com"));

            // create updated users
            User userAuthorized = UserFactory.CreateNormalUser(4);
            userAuthorized.Email = "new4@mail.com";
            User userNotAuthorized = UserFactory.CreateNormalUser(2000);
            userNotAuthorized.Email = "new2000@mail.com";

            // create put requests
            HttpRequest requestAuthorized = HttpRequestFactory.CreatePutRequest<User>(userAuthorized);
            HttpRequest requestNotAuthorized = HttpRequestFactory.CreatePutRequest<User>(userNotAuthorized);

            ObjectResult resultAuthorized = (ObjectResult)await this.userController.UserUpdate(requestAuthorized, 4, this.userClaim);
            ObjectResult resultNotAuthorized = (ObjectResult)await this.userController.UserUpdate(requestNotAuthorized, 2000, this.userClaim);

            UserResponse resultUserAuthorized = (UserResponse)resultAuthorized.Value;

            // status code should be 200 OK
            Assert.Equal(200, resultAuthorized.StatusCode);
            // status code should be 403 FORBIDDEN
            Assert.Equal(403, resultNotAuthorized.StatusCode);
            
            // the email should be updated
            Assert.Equal("new4@mail.com", resultUserAuthorized.Email);
            // the email should not be updated
            Assert.Equal("old2@mail.com", (await this.userRepository.ReadAsync(2000)).Email);
        }

        [Fact]
        public async void UpdateUserAsAdminTest()
        {
            // create user
            await this.userRepository.CreateAsync(UserFactory.CreateNormalUser(2000));

            // create updated user
            User user = UserFactory.CreateNormalUser(2000);
            user.Email = "new4@mail.com";

            // create put request
            HttpRequest request = HttpRequestFactory.CreatePutRequest<User>(user);

            ObjectResult result = (ObjectResult)await this.userController.UserUpdate(request, 2000, this.adminClaim);

            UserResponse resultUser = (UserResponse)result.Value;

            // status code should be 200 OK
            Assert.Equal(200, result.StatusCode);
            // the email should be updated
            Assert.Equal("new4@mail.com", resultUser.Email);
        }

        [Fact]
        public async void DeleteUserAsAdminTest()
        {
            // create user
            await this.userRepository.CreateAsync(UserFactory.CreateNormalUser(2000));

            // database should contain 1 user
            Assert.Single(await this.userRepository.ListAsync());

            // create delete request
            HttpRequest request = HttpRequestFactory.CreateDeleteRequest();

            StatusCodeResult result = (StatusCodeResult)await this.userController.UserDelete(request, 2000, this.adminClaim);

            // status code should be 200 OK
            Assert.Equal(200, result.StatusCode);
            // the account should be removed
            Assert.Empty(await this.userRepository.ListAsync());
        }

        [Fact]
        public async void DeleteUserAsNonAdminTest()
        {
            // create user
            await this.userRepository.CreateAsync(UserFactory.CreateNormalUser(2000));

            // database should contain 1 user
            Assert.Single(await this.userRepository.ListAsync());

            HttpRequest request = HttpRequestFactory.CreateDeleteRequest();

            ObjectResult resultUser = (ObjectResult)await this.userController.UserDelete(request, 2000, this.userClaim);
            ObjectResult resultOrganiser = (ObjectResult)await this.userController.UserDelete(request, 2000, this.organiserClaim);

            ErrorResponse errorMessageUser = (ErrorResponse)resultUser.Value;
            ErrorResponse errorMessageOrganiser = (ErrorResponse)resultOrganiser.Value;

            // status code should be 403 FORBIDDEN
            Assert.Equal(403, resultUser.StatusCode);
            Assert.Equal(403, resultOrganiser.StatusCode);

            // error code must be unauthorized because non admin user can only edit or delete own account
            Assert.Equal(ErrorCode.UNAUTHORIZED_TO_DELETE_USER, errorMessageUser.ErrorCodeEnum);
            Assert.Equal(ErrorCode.UNAUTHORIZED_TO_DELETE_USER, errorMessageOrganiser.ErrorCodeEnum);
            // database should still contain 1 user
            Assert.Single(await this.userRepository.ListAsync());
        }

        [Fact]
        public async void SendRequestWithoutJsonBodyTest()
        {
            HttpRequest request = HttpRequestFactory.CreatePostRequestWithoutBody();

            ObjectResult result = (ObjectResult)await this.userController.UserCreate(request, this.userClaim);

            // status code should be 400 BAD REQUEST
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async void CreateUserWithInvalidNameTest()
        {
            // too long name
            User userLong = UserFactory.CreateNormalUser();
            userLong.Name = "toolongname000000000000000000000000000000000";
            HttpRequest requestLong = HttpRequestFactory.CreatePostRequest<User>(userLong);
            ObjectResult resultLong = (ObjectResult)await this.userController.UserCreate(requestLong, this.adminClaim);

            // too short name
            User userShort = UserFactory.CreateNormalUser();
            userShort.Name = "0";
            HttpRequest requestShort = HttpRequestFactory.CreatePostRequest<User>(userShort);
            ObjectResult resultShort = (ObjectResult)await this.userController.UserCreate(requestShort, this.adminClaim);

            // status code should be 400 BAD REQUEST
            Assert.Equal(400, resultLong.StatusCode);
            Assert.Equal(400, resultShort.StatusCode);
        }

        [Fact]
        public async void CreateUserWithInvalidPasswordTest()
        {
            // too short password
            var jsonDictShort = new Dictionary<string, object>{
                { "name", "Pete" },
                { "email", "pete@mail.com" },
                { "password", "1234" },
                { "dateofbirth", "2000-01-01" },
                { "gender", 0 }
            };
            var jsonShort = JsonConvert.SerializeObject(jsonDictShort, Formatting.Indented);
            HttpRequest requestShort = HttpRequestFactory.CreatePostRequest(jsonShort);
            ObjectResult resultShort = (ObjectResult)await this.userController.UserCreate(requestShort, this.adminClaim);

            // status code should be 400 BAD REQUEST
            Assert.Equal(400, resultShort.StatusCode);
        }

        [Fact]
        public async void CreateUserWithInvalidDateofbirthTest()
        {
            // invalid date of birth
            var jsonDictInvalid = new Dictionary<string, object>{
                { "name", "Pete" },
                { "email", "pete@mail.com" },
                { "password", "10290292034" },
                { "dateofbirth", "invaliddateofbirth" },
                { "gender", 0 },
                { "type", 0 }
            };
            var jsonInvalid = JsonConvert.SerializeObject(jsonDictInvalid, Formatting.Indented);
            HttpRequest requestInvalid = HttpRequestFactory.CreatePostRequest(jsonInvalid);
            ObjectResult resultInvalid = (ObjectResult)await this.userController.UserCreate(requestInvalid, this.adminClaim);

            // date of birth in the future
            User userFuture = UserFactory.CreateNormalUser();
            userFuture.Dateofbirth = DateTime.Now.AddYears(1);
            HttpRequest requestFuture = HttpRequestFactory.CreatePostRequest<User>(userFuture);
            ObjectResult resultFuture = (ObjectResult)await this.userController.UserCreate(requestFuture, this.adminClaim);

            // status code should be 400 BAD REQUEST
            Assert.Equal(400, resultInvalid.StatusCode);
            Assert.Equal(400, resultFuture.StatusCode);
        }

        [Fact]
        public async void CreateUserWithoutNameFieldTest()
        {
            // create json
            var jsonDict = new Dictionary<string, object>{
                { "email", "pete@mail.com" },
                { "password", "10290292034" },
                { "dateofbirth", "2000-01-01" },
                { "gender", 0 },
                { "type", 0 }
            };
            var json = JsonConvert.SerializeObject(jsonDict, Formatting.Indented);

            // create request
            HttpRequest request = HttpRequestFactory.CreatePostRequest(json);

            ObjectResult result = (ObjectResult)await this.userController.UserCreate(request, this.adminClaim);

            // status code should be 400 BAD REQUEST
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async void CreateUserWithoutEmailFieldTest()
        {
            // create json
            var jsonDict = new Dictionary<string, object>{
                { "name", "Pete" },
                { "password", "10290292034" },
                { "dateofbirth", "2000-01-01" },
                { "gender", 0 },
                { "type", 0 }
            };
            var json = JsonConvert.SerializeObject(jsonDict, Formatting.Indented);

            // create request
            HttpRequest request = HttpRequestFactory.CreatePostRequest(json);

            ObjectResult result = (ObjectResult)await this.userController.UserCreate(request, this.adminClaim);

            // status code should be 400 BAD REQUEST
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async void CreateUserWithoutPasswordFieldTest()
        {
            // create json
            var jsonDict = new Dictionary<string, object>{
                { "name", "Pete" },
                { "email", "pete@mail.com" },
                { "dateofbirth", "2000-01-01" },
                { "gender", 0 },
                { "type", 0 }
            };
            var json = JsonConvert.SerializeObject(jsonDict, Formatting.Indented);

            // create request
            HttpRequest request = HttpRequestFactory.CreatePostRequest(json);

            ObjectResult result = (ObjectResult)await this.userController.UserCreate(request, this.adminClaim);

            // status code should be 400 BAD REQUEST
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async void CreateUserWithoutDateofbirthFieldTest()
        {
            // create json
            var jsonDict = new Dictionary<string, object>{
                { "name", "Pete" },
                { "email", "pete@mail.com" },
                { "password", "10290292034" },
                { "gender", 0 },
                { "type", 0 }
            };
            var json = JsonConvert.SerializeObject(jsonDict, Formatting.Indented);

            // create request
            HttpRequest request = HttpRequestFactory.CreatePostRequest(json);

            ObjectResult result = (ObjectResult)await this.userController.UserCreate(request, this.adminClaim);

            // status code should be 400 BAD REQUEST
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async void CreateUserWithoutGenderFieldTest()
        {
            // create json
            var jsonDict = new Dictionary<string, object>{
                { "name", "Pete" },
                { "email", "pete@mail.com" },
                { "password", "10290292034" },
                { "dateofbirth", "2000-01-01" },
                { "type", 0 }
            };
            var json = JsonConvert.SerializeObject(jsonDict, Formatting.Indented);

            // create request
            HttpRequest request = HttpRequestFactory.CreatePostRequest(json);

            ObjectResult result = (ObjectResult)await this.userController.UserCreate(request, this.adminClaim);

            // status code should be 400 BAD REQUEST
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async void CreateUserWithoutTypeFieldTest()
        {
            // create json
            var jsonDict = new Dictionary<string, object>{
                { "name", "Pete" },
                { "email", "pete@mail.com" },
                { "password", "10290292034" },
                { "dateofbirth", "2000-01-01" },
                { "gender", 0 }
            };
            var json = JsonConvert.SerializeObject(jsonDict, Formatting.Indented);

            // create request
            HttpRequest request = HttpRequestFactory.CreatePostRequest(json);

            ObjectResult result = (ObjectResult)await this.userController.UserCreate(request, this.adminClaim);

            // status code should be 400 OK
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async void CreateUserWithInvalidGenderValueTest()
        {
            // create json
            var jsonDict = new Dictionary<string, object>{
                { "name", "Pete" },
                { "email", "pete@mail.com" },
                { "password", "10290292034" },
                { "dateofbirth", "2000-01-01" },
                { "gender", 10 },
                { "type", 0 }
            };
            var json = JsonConvert.SerializeObject(jsonDict, Formatting.Indented);

            // create request
            HttpRequest request = HttpRequestFactory.CreatePostRequest(json);

            ObjectResult result = (ObjectResult)await this.userController.UserCreate(request, this.adminClaim);

            // status code should be 400 BAD REQUEST
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async void CreateUserWithInvalidUserTypeValueTest()
        {
            // create json
            var jsonDict = new Dictionary<string, object>{
                { "name", "Pete" },
                { "email", "pete@mail.com" },
                { "password", "10290292034" },
                { "dateofbirth", "2000-01-01" },
                { "gender", 0 },
                { "type", 10 }
            };
            var json = JsonConvert.SerializeObject(jsonDict, Formatting.Indented);

            // create request
            HttpRequest request = HttpRequestFactory.CreatePostRequest(json);

            ObjectResult result = (ObjectResult)await this.userController.UserCreate(request, this.adminClaim);

            // status code should be 400 BAD REQUEST
            Assert.Equal(400, result.StatusCode);
        }
    }
}
