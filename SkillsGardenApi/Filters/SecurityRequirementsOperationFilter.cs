using System;
using System.Collections.Generic;
using System.Reflection;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SkillsGardenApi.Filters {
	public class SecurityRequirementsOperationFilter : IOperationFilter {
		public void Apply(OpenApiOperation Operation, OperationFilterContext Context) {
			if (GetAuthorizationLevel(Context) == AuthorizationLevel.User) {
				Operation.Responses["401"] = new OpenApiResponse { Description = "Unauthorized" };
				Operation.Responses["403"] = new OpenApiResponse { Description = "Forbidden" };

				OpenApiSecurityScheme Scheme = new OpenApiSecurityScheme {
					Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "BearerAuth" }
				};

				Operation.Security = new List<OpenApiSecurityRequirement>{
					new OpenApiSecurityRequirement{
					  [Scheme] = new List<string>()
					}
				};
			}
		}

		AuthorizationLevel GetAuthorizationLevel(OperationFilterContext Context) {
			foreach (ParameterInfo Parameter in Context.MethodInfo.GetParameters()) {
				HttpTriggerAttribute Attribute = Parameter.GetCustomAttribute<HttpTriggerAttribute>();

				if (Attribute != null) {
					return Attribute.AuthLevel;
				}
			}

			throw new Exception();
		}
	}
}