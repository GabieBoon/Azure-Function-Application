using Microsoft.AspNetCore.Http;
using SkillsGardenDTO.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SkillsGardenDTO
{
    /// <summary>
    /// The DTO for component
    /// </summary>
    public class ComponentBody
    {
        /// <summary>
        /// The name of the component
        /// </summary>
        /// <example>Stippeltrap</example>
        [MinLength(2, ErrorMessage = "Name must be at least 2 characters")]
        [MaxLength(50, ErrorMessage = "Name can not be longer than 50 characters")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        /// <summary>
        /// The description of the component
        /// </summary>
        /// <example>Trap</example>
        [MinLength(2, ErrorMessage = "Description must be at least 2 characters")]
        [MaxLength(500, ErrorMessage = "Description can not be longer than 500 characters")]
        [DataType(DataType.Text)]
        public string Description { get; set; }

        /// <summary>
        /// The image for the component
        /// </summary>
        /// <example>image</example>
        [DataType(DataType.Upload)]
        [MaxFileSize(10 * 1024 * 1024)]
        [AllowedExtensions(new string[] { ".jpg", ".jpeg", ".png" })]
        public FormFile Image { get; set; }

        /// <summary>
        /// The exercises for the component
        /// </summary>
        /// <example>array</example>
        public List<int> Exercises { get; set; }
    }
}