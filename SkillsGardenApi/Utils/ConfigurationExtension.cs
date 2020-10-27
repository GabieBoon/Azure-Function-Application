using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SkillsGardenApi.Utils
{
    public static class ConfigurationExtension
    {
        public static T GetClassValueChecked<T>(this IConfiguration Configuration,
                                                string Key, T Default, ILogger Logger) where T : class
        {
            T Value = Configuration.GetValue<T>(Key);

            if (Value == null)
            {
                Logger.LogError($"Configuration key {Key} not found. Check your configuration!");

                return Default;
            }

            return Value;
        }

        public static T GetValueChecked<T>(this IConfiguration Configuration,
                                           string Key, T Default, ILogger Logger) where T : struct
        {
            T? Value = Configuration.GetValue<T?>(Key);

            if (!Value.HasValue)
            {
                Logger.LogError($"Configuration key {Key} not found. Check your configuration!");

                return Default;
            }

            return Value.Value;
        }
    }
}