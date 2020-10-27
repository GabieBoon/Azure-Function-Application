using Dahomey.Json.Attributes;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace SkillsGardenApi.Filters {
    public class SwaggerRequiredFilter : ISchemaFilter {
        private string GetPropertyName(string Name) {
            return $"{Name.Substring(0, 1).ToLower()}{Name.Substring(1)}";
        }

        public void Apply(OpenApiSchema Schema, SchemaFilterContext Context) {
            if (Schema.Properties.Count == 0) {
                return;
            }

            PropertyInfo[] Properties = Context.Type.GetProperties();

            foreach (PropertyInfo Property in Properties) {
                string PropertyName = GetPropertyName(Property.Name);
                JsonRequiredAttribute Attribute = Property.GetCustomAttribute<JsonRequiredAttribute>();

                ApplyProperty(Schema, PropertyName, Attribute);
            }
        }

        private void ApplyProperty(OpenApiSchema Schema, string PropertyName, JsonRequiredAttribute Attribute) {
            OpenApiSchema PropertySchema = Schema.Properties[PropertyName];

            if (Attribute != null) {
                if (Attribute.Policy == RequirementPolicy.Always) {
                    Schema.Required.Add(PropertyName);
                    PropertySchema.Nullable = false;
                }
                else if (Attribute.Policy == RequirementPolicy.AllowNull) {
                    Schema.Required.Add(PropertyName);
                    PropertySchema.Nullable = true;
                }
                else if (Attribute.Policy == RequirementPolicy.DisallowNull) {
                    PropertySchema.Nullable = false;
                }
                else if (Attribute.Policy == RequirementPolicy.Never) {
                    PropertySchema.Nullable = true;
                }
            }
        }
    }
}