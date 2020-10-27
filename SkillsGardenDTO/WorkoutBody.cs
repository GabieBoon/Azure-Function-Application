using Microsoft.AspNetCore.Http;
using SkillsGardenDTO.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SkillsGardenDTO
{
    /// <summary>
    /// The DTO for workout
    /// </summary>
    public class WorkoutBody
    {
        /// <summary>
        /// The name of the workout
        /// </summary>
        /// <example>Krachttraining</example>
        [MinLength(2, ErrorMessage = "Name must be at least 2 characters")]
        [MaxLength(50, ErrorMessage = "Name can not be longer than 50 characters")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        /// <summary>
        /// The type of the workout
        /// </summary>
        /// <example>Balans</example>
        [MinLength(2, ErrorMessage = "Type must be at least 2 characters")]
        [MaxLength(50, ErrorMessage = "Type can not be longer than 50 characters")]
        [DataType(DataType.Text)]
        public string Type { get; set; }

        /// <summary>
        /// The exercises for the workout
        /// </summary>
        /// <example>array</example>
        public List<int> Exercises { get; set; }
    }
}