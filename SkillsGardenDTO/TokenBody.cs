using Dahomey.Json.Attributes;
using SkillsGardenDTO.Response;

namespace SkillsGardenDTO {
    /// <summary>
    /// The DTO for the token
    /// </summary>
    public class TokenBody
    {
        /// <summary>
        /// The bearer token
        /// </summary>
        /// <example></example>
        [JsonRequired(RequirementPolicy.Always)]
        public string Token { get; set; }

        /// <summary>
        /// The logged in user
        /// </summary>
        /// <example></example>
        [JsonRequired(RequirementPolicy.Always)]
        public UserResponse User { get; set; }
    }
}