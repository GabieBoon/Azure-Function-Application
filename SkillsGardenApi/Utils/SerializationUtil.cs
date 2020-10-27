using Dahomey.Json;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SkillsGardenApi.Utils
{
    public static class SerializationUtil
    {
        public async static Task<T> Deserialize<T>(Stream body)
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            options.PropertyNameCaseInsensitive = true;

            T model = await JsonSerializer.DeserializeAsync<T>(body, options);

            ValidationContext context = new ValidationContext(model, null, null);
            List<ValidationResult> validationResults = new List<ValidationResult>();
            bool valid = Validator.TryValidateObject(model, context, validationResults, true);
            if (!valid)
            {
                throw new JsonException(validationResults[0].ErrorMessage);
            }

            return model;
        }

        public static T DeserializeFormData<T>(IFormCollection formdata)
        {
            T model = GetObject<T>();
            var modeltype = model.GetType();

            foreach (var item in formdata)
            {
                if(modeltype.GetProperty(item.Key) == null) throw new ValidationException($"{item.Key} is not a valid field");

                dynamic value = item.Value.ToString();

                // custom parse for int values
                if (modeltype.GetProperty(item.Key).PropertyType == typeof(int) || modeltype.GetProperty(item.Key).PropertyType == typeof(int?))
                {
                    int intValue;
                    if (!Int32.TryParse(value, out intValue)) throw new ValidationException($"{item.Key} must be an integer");
                    value = intValue;
                }
                // custom parse for double values
                else if (modeltype.GetProperty(item.Key).PropertyType == typeof(double) || modeltype.GetProperty(item.Key).PropertyType == typeof(double?))
                {
                    double doublevalue;
                    if (!Double.TryParse(value.Replace('.', ','), out doublevalue)) throw new ValidationException($"{item.Key} must be a double");
                    value = doublevalue;
                }
                // custom parse for datetime values
                else if (modeltype.GetProperty(item.Key).PropertyType == typeof(DateTime) || modeltype.GetProperty(item.Key).PropertyType == typeof(DateTime?))
                {
                    DateTime dateTimeValue;
                    if (!DateTime.TryParse(value, out dateTimeValue)) throw new ValidationException($"{item.Key} must be a datetime");
                    value = dateTimeValue;
                }
                // custom parse for list with int values
                else if (modeltype.GetProperty(item.Key).PropertyType == typeof(List<int>))
                {
                    try {
                        List<string> listValue = new List<string>(value.Split(','));
                        value = listValue.Select(x => int.Parse(x.Trim())).ToList();
                    }
                    catch (Exception e) {
                        throw new ValidationException($"{item.Key} must be an array of integers");
                    }
                }

                modeltype.GetProperty(item.Key).SetValue(model, value);
            }

            foreach(var file in formdata.Files)
            {
                if (modeltype.GetProperty(file.Name) == null) throw new ValidationException($"{file.Name} is not a valid image field");
                modeltype.GetProperty(file.Name).SetValue(model, file);
            }

            ValidationContext context = new ValidationContext(model, null, null);
            List<ValidationResult> validationResults = new List<ValidationResult>();
            bool valid = Validator.TryValidateObject(model, context, validationResults, true);
            if (!valid)
            {
                throw new ValidationException(validationResults[0].ErrorMessage);
            }

            return model;
        }

        private static T GetObject<T>(params object[] lstArgument)
        {
            return (T)Activator.CreateInstance(typeof(T), lstArgument);
        }
    }
}
