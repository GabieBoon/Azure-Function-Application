using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SkillsGardenDTO
{
    /// <summary>
    /// The DTO for exercise
    /// </summary>
    public class ExerciseBody
    {
        /// <summary>
        /// The name of the exercise
        /// </summary>
        /// <example>Pete</example>
        [MinLength(2, ErrorMessage = "Name must be at least 2 characters")]
        [MaxLength(30, ErrorMessage = "Name can not be longer than 30 characters")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        /// <summary>
        /// The requirements of the exercise
        /// </summary>
        /// <example>array</example>
        public List<string> Requirements { get; set; }

        /// <summary>
        /// The setps of the exercise
        /// </summary>
        /// <example>array</example>
        public List<string> Steps { get; set; }

        /// <summary>
        /// The movement forms of the exercise
        /// </summary>
        /// <example>array</example>
        public List<MovementForm> Forms { get; set; }

        /// <summary>
        /// The media for the exercise
        /// </summary>
        /// <example>http://asmplatform.nl/video</example>
        public string Media { get; set; }
    }

    /// <summary>
    /// The movement forms
    /// </summary>
    public enum MovementForm
    {
        /// <summary>
        /// Klimmen
        /// </summary>
        klimmen,
        /// <summary>
        /// Balans
        /// </summary>
        balans,
        /// <summary>
        /// Mikken
        /// </summary>
        mikken,
        /// <summary>
        /// Gooien
        /// </summary>
        gooien,
        /// <summary>
        /// Springen
        /// </summary>
        springen,
        /// <summary>
        /// Zwaaien
        /// </summary>
        zwaaien,
        /// <summary>
        /// Rollen
        /// </summary>
        rollen,
        /// <summary>
        /// Hardlopen
        /// </summary>
        hardlopen,
        /// <summary>
        /// Overspelen
        /// </summary>
        overspelen,
        /// <summary>
        /// Stoeien
        /// </summary>
        stoeien
    }
}
