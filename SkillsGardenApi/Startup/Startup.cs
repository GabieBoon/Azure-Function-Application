using AzureFunctions.Extensions.Swashbuckle;
using AzureFunctions.Extensions.Swashbuckle.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using SkillsGardenApi.Filters;
using SkillsGardenApi.Models;
using SkillsGardenApi.Repositories;
using SkillsGardenApi.Repositories.Context;
using SkillsGardenApi.Security;
using SkillsGardenApi.Services;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.IO;
using System.Reflection;

[assembly: FunctionsStartup(typeof(SkillsGardenApi.Startup))]
namespace SkillsGardenApi
{
    public class Startup : FunctionsStartup {
		public override void Configure(IFunctionsHostBuilder Builder)
		{
			ConfigureServices(Builder);
			ConfigureSwagger(Builder);
			ConfigureAuthenticaton(Builder);
			ConfigureAuthorization(Builder);
			ConfigureDatabase(Builder);
		}

		private void ConfigureServices(IFunctionsHostBuilder Builder)
		{
			Builder.Services.AddSingleton<ITokenService, TokenService>();
			Builder.Services.AddSingleton<IAzureService, AzureService>();
			Builder.Services.AddScoped<AuthService>();
			Builder.Services.AddScoped<UserService>();
			Builder.Services.AddScoped<ExerciseService>();
			Builder.Services.AddScoped<EventService>();
			Builder.Services.AddScoped<WorkoutService>();
			Builder.Services.AddScoped<LocationService>();
			Builder.Services.AddScoped<ComponentService>();
			Builder.Services.AddScoped<BeaconService>();

		}

		private void ConfigureDatabase(IFunctionsHostBuilder Builder)
		{
			var connectionString = Environment.GetEnvironmentVariable("SqlConnectionString");
			Builder.Services.AddDbContext<DatabaseContext>(x =>
			{
				x.UseSqlServer(connectionString, options => options.EnableRetryOnFailure());
			});

			Builder.Services.AddTransient<IDatabaseRepository<Location>, LocationRepository>();
			Builder.Services.AddTransient<IDatabaseRepository<User>, UserRepository>();
			Builder.Services.AddTransient<IDatabaseRepository<Component>, ComponentRepository>();
			Builder.Services.AddTransient<IDatabaseRepository<Event>, EventRepository>();
			Builder.Services.AddTransient<IDatabaseRepository<Exercise>, ExerciseRepository>();
			Builder.Services.AddTransient<IDatabaseRepository<Beacon>, BeaconRepository>();
			Builder.Services.AddTransient<IDatabaseRepository<Workout>, WorkoutRepository>();
		}

		private void ConfigureAuthenticaton(IFunctionsHostBuilder Builder)
		{
			Builder.Services.AddAuthentication((AuthenticationOptions Options) => {
				Options.AddScheme<WebJobsAuthLevelHandler>(SecurityDefinition.WebJobsAuthLevel.ToString(), "");
				Options.AddScheme<BearerAuthenticationHandler>(SecurityDefinition.Bearer.ToString(), "");
			});
		}

		private void ConfigureAuthorization(IFunctionsHostBuilder Builder)
		{
			Builder.Services.AddAuthorization((AuthorizationOptions Options) => {
				//
			});

			Builder.Services.AddSingleton<IAuthorizationService, BearerAuthorizationService>();
		}

		private void ConfigureSwagger(IFunctionsHostBuilder Builder)
		{
			Builder.AddSwashBuckle(Assembly.GetExecutingAssembly(), (SwaggerDocOptions Options) => {
				Options.SpecVersion = OpenApiSpecVersion.OpenApi3_0;
				Options.AddCodeParameter = false;
				Options.PrependOperationWithRoutePrefix = true;
				Options.XmlPath = "SkillsGardenApi.xml";

				Options.Documents = new[] {
					new SwaggerDocument{
						Name = "v1",
						Title = "Skills Garden API",
						Description = "Skills Garden API",
						Version = "v1"
					}
				};

				Options.Title = "Swagger Specification";
				Options.ConfigureSwaggerGen = (SwaggerGenOptions Options) => {
					Options.IncludeXmlComments(GetLocalFilename("SkillsGardenDTO.xml"));

					Options.OperationFilter<SecurityRequirementsOperationFilter>();
					Options.SchemaFilter<SwaggerRequiredFilter>();
					Options.OperationFilter<SwaggerFormDataFilter>();

					Options.CustomOperationIds((ApiDescription ApiDesc) => {
						MethodInfo MethodInfo;
						if (ApiDesc.TryGetMethodInfo(out MethodInfo))
						{
							return MethodInfo.Name;
						}
						else
						{
							return new Guid().ToString();
						}
					});

					OpenApiSecurityScheme SecurityScheme = new OpenApiSecurityScheme();
					SecurityScheme.Type = SecuritySchemeType.Http;
					SecurityScheme.Scheme = "bearer";
					SecurityScheme.Description = "JWT for authorization";
					SecurityScheme.BearerFormat = "JWT";
					Options.AddSecurityDefinition("BearerAuth", SecurityScheme);
				};
			});
		}

		private string GetLocalFilename(string Filename)
		{
			string AssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			DirectoryInfo AssemblyDirectory = Directory.CreateDirectory(AssemblyPath);

			string BasePath = AssemblyDirectory?.Parent?.FullName;

			return Path.Combine(BasePath, Filename);
		}
	}
}
