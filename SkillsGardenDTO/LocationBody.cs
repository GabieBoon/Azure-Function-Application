using Microsoft.AspNetCore.Http;
using SkillsGardenDTO.Attributes;
using System.ComponentModel.DataAnnotations;

namespace SkillsGardenDTO
{
    /// <summary>
    /// The DTO for location
    /// </summary>
    public class LocationBody
    {
        /// <summary>
        /// The name of the location
        /// </summary>
        /// <example>Almere Haven</example>
        [MinLength(2, ErrorMessage = "Name must be at least 2 characters")]
        [MaxLength(50, ErrorMessage = "Name can not be longer than 50 characters")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        /// <summary>
        /// The city of the location
        /// </summary>
        /// <example>Almere</example>
        [MinLength(2, ErrorMessage = "City must be at least 2 characters")]
        [MaxLength(50, ErrorMessage = "City can not be longer than 50 characters")]
        [DataType(DataType.Text)]
        public string City { get; set; }

        /// <summary>
        /// The latitude of the location of the Skills Garden
        /// </summary>
        /// <example>40.201</example>
        [DataType(DataType.Text)]
        public double? Lat { get; set; }

        /// <summary>
        /// The longitude of the location of the Skills Garden
        /// </summary>
        /// <example>2.0</example>
        [DataType(DataType.Text)]
        public double? Lng { get; set; }

        /// <summary>
        /// The image of the Skills Garden
        /// </summary>
        /// <example>image</example>
        [DataType(DataType.Upload)]
        [MaxFileSize(10 * 1024 * 1024)]
        [AllowedExtensions(new string[] { ".jpg", ".jpeg", ".png" })]
        public FormFile Image { get; set; }
    }
}