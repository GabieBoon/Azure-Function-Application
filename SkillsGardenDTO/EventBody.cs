using Microsoft.AspNetCore.Http;
using SkillsGardenDTO.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace SkillsGardenDTO
{
    /// <summary>
    /// The DTO of event
    /// </summary>
    public class EventBody
    {
        /// <summary>
        /// The title of the event
        /// </summary>
        /// <example>Bootcamp</example>
        [MinLength(2, ErrorMessage = "Title must be at least 2 characters")]
        [MaxLength(50, ErrorMessage = "Title can not be longer than 50 characters")]
        [DataType(DataType.Text)]
        public string Title { get; set; }

        /// <summary>
        /// The description of the event
        /// </summary>
        /// <example>Kom mee op bootcamp</example>
        [MinLength(2, ErrorMessage = "Description must be at least 2 characters")]
        [MaxLength(50, ErrorMessage = "Description can not be longer than 500 characters")]
        [DataType(DataType.Text)]
        public string Description { get; set; }

        /// <summary>
        /// The start time of the event
        /// </summary>
        /// <example>2020-01-01T20:00</example>
        [DataType(DataType.DateTime)]
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// The max amount of registrations for the event
        /// </summary>
        /// <example>20</example>
        [Range(0, Int32.MaxValue)]
        public int? MaxRegistrations { get; set; }

        /// <summary>
        /// The image for the event
        /// </summary>
        /// <example></example>
        [DataType(DataType.Upload)]
        [MaxFileSize(10 * 1024 * 1024)]
        [AllowedExtensions(new string[] { ".jpg", ".jpeg", ".png" })]
        public FormFile Image { get; set; }
    }
}
