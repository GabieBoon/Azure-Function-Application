using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SkillsGardenApi.Filters
{
    class SwaggerFormDataFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // check if annotation was used
            var isFormDataOperation = context.MethodInfo.CustomAttributes.Any(a => a.AttributeType == typeof(FormDataItem));
            if (!isFormDataOperation) return;

            // get annotation data
            var formDataItems = context.MethodInfo.DeclaringType.GetCustomAttributes(true)
                .Union(context.MethodInfo.GetCustomAttributes(true))
                .OfType<FormDataItem>();

            // setup the reqest body
            operation.RequestBody = new OpenApiRequestBody
            {
                Content =
                {
                    ["multipart/form-data"] = new OpenApiMediaType()
                    {
                        Schema = new OpenApiSchema()
                        {
                            Type = "object",
                            Properties = {}
                        }
                    }
                }
            };

            // get the schema
            var schema = operation.RequestBody.Content["multipart/form-data"].Schema.Properties;

            // add the form data items
            foreach (FormDataItem item in formDataItems)
            {
                OpenApiSchema openApiSchema = new OpenApiSchema();
                openApiSchema.Description = item.Description;
                openApiSchema.Type = item.Type;
                if (item.Format != null) openApiSchema.Format = item.Format;

                schema[item.Name] = openApiSchema;
            }
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class FormDataItem : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Format { get; set; }
    }
}
