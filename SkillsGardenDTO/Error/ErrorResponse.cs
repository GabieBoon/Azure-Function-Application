using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Reflection;

namespace SkillsGardenDTO.Error
{
    public class ErrorResponse
    {
        public int Code { get; }
        public string Message { get; }

        [JsonIgnore]
        public ErrorCode? ErrorCodeEnum;

        public ErrorResponse(ErrorCode errorCode)
        {
            this.Code = (int)errorCode;
            this.Message = GetDescription(errorCode);
            this.ErrorCodeEnum = errorCode;
        }

        public ErrorResponse(int code, string messsage)
        {
            this.Code = code;
            this.Message = messsage;
        }

        private string GetDescription(ErrorCode errorCode)
        {
            // get type of enum
            Type type = errorCode.GetType();
            // get value of enum
            string name = Enum.GetName(type, errorCode);

            // get the field of the value
            FieldInfo field = type.GetField(name);
            // get the description attribute
            DescriptionAttribute attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
            return attr.Description;
        }
    }
}
