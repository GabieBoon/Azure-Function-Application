using AzureFunctions.Extensions.Swashbuckle.Attribute;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using SkillsGardenApi.Models;
using SkillsGardenApi.Security;
using SkillsGardenApi.Services;
using SkillsGardenApi.Utils;
using SkillsGardenDTO;
using SkillsGardenDTO.Error;
using SkillsGardenDTO.Response;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace SkillsGardenApi.Controllers
{
    public class AuthController 
    {
        private ITokenService tokenService;
        private AuthService authService;
        private UserService userService;

        public AuthController(ITokenService tokenService, AuthService authService, UserService userService)
        {
            this.tokenService = tokenService;
            this.authService = authService;
            this.userService = userService;
        }

        /// <summary>
		/// Get bearer token for API
		/// </summary>
        [FunctionName("Login")]
        [ProducesResponseType(typeof(TokenBody), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "login")]
            [RequestBodyType(typeof(LoginBody), "Credentials")] LoginBody loginBody)
        {
            // check if all fields are filled in
            if (loginBody.Email == null || loginBody.Password == null)
                return new BadRequestObjectResult(new ErrorResponse(ErrorCode.INVALID_REQUEST_BODY));

            // verify login credentials
            User user = await authService.VerifyLogin(loginBody);

            // if the login failed
            if (user == null)
                return new UnauthorizedObjectResult(new ErrorResponse(ErrorCode.INVALID_COMBINATION_OF_EMAIL_AND_PASSWORD));

            // get token
            Token token = await tokenService.CreateToken(user);

            // get the created user
            UserResponse response = await userService.GetUser(user.Id);

            return new OkObjectResult(new TokenBody
            {
                Token = token.Value,
                User = response
            });
        }

        [FunctionName("Register")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Register(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "register")]
            [RequestBodyType(typeof(UserBody), "The user to create")] HttpRequest req)
        {
            // deserialize request
            UserBody userBody;
            try
            {
                userBody = await SerializationUtil.Deserialize<UserBody>(req.Body);
            }
            catch (JsonException e)
            {
                return new BadRequestObjectResult(new ErrorResponse(400, e.Message));
            }

            // check for required fields
            if (userBody.Name == null) return new BadRequestObjectResult(new ErrorResponse(400, "Name is required"));
            if (userBody.Email == null) return new BadRequestObjectResult(new ErrorResponse(400, "Email is required"));
            if (userBody.Password == null) return new BadRequestObjectResult(new ErrorResponse(400, "Password is required"));
            if (userBody.Dateofbirth == null) return new BadRequestObjectResult(new ErrorResponse(400, "Dateofbirth is required"));
            if (userBody.Gender == null) return new BadRequestObjectResult(new ErrorResponse(400, "Gender is required"));

            // check for date of birth not in the future    
            if (userBody.Dateofbirth >= DateTime.Now) return new BadRequestObjectResult(new ErrorResponse(400, "Dateofbirth can not be in the future"));

            // cannot set user type
            if (userBody.Type != null && userBody.Type != UserType.User)
                return ForbiddenObjectResult.Create(new ErrorResponse(ErrorCode.UNAUTHORIZED_TO_SET_USER_TYPE));

            // check if there is already an user with this email
            if (!await userService.EmailExists(userBody.Email))
                return new BadRequestObjectResult(new ErrorResponse(400, "Email does already exist"));

            // create user
            User createdUser = await this.userService.CreateUser(userBody);

            // get the created user
            UserResponse response = await userService.GetUser(createdUser.Id);

            return new OkObjectResult(response);
        }
    }
}
