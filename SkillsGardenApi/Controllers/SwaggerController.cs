using AzureFunctions.Extensions.Swashbuckle;
using AzureFunctions.Extensions.Swashbuckle.Attribute;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Net.Http;
using System.Threading.Tasks;

namespace SkillsGardenApi.Controllers
{
    public class SwaggerController
    {
		[AllowAnonymous]
		[SwaggerIgnore]
		[FunctionName("Swagger")]
		public Task<HttpResponseMessage> Swagger([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Swagger/json")] HttpRequestMessage Request, [SwashBuckleClient] ISwashBuckleClient SwashBuckleClient)
		{
			return Task.FromResult(SwashBuckleClient.CreateSwaggerDocumentResponse(Request));
		}

		[AllowAnonymous]
		[SwaggerIgnore]
		[FunctionName("SwaggerUi")]
		public Task<HttpResponseMessage> SwaggerUi([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Swagger/ui")] HttpRequestMessage Request, [SwashBuckleClient] ISwashBuckleClient SwashBuckleClient)
		{
			return Task.FromResult(SwashBuckleClient.CreateSwaggerUIResponse(Request, "swagger/json"));
		}
	}
}
