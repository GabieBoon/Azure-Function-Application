using Dahomey.Json.Attributes;

namespace SkillsGardenDTO {
    /// <summary>
    /// The DTO for supplying login information
    /// </summary>
    public class LoginBody
    {
        /// <summary>
        /// The email of the user
        /// </summary>
        /// <example>admin@admin.nl</example>
        [JsonRequired(RequirementPolicy.Always)]
        public string Email { get; set; }

        /// <summary>
        /// The password of the user
        /// </summary>
        /// <example>adminadmin</example>
        [JsonRequired(RequirementPolicy.Always)]
        public string Password { get; set; }
    }
}