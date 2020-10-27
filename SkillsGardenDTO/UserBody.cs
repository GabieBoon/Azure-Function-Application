using System;
using System.ComponentModel.DataAnnotations;

namespace SkillsGardenDTO
{
    /// <summary>
    /// The DTO for user
    /// </summary>
    public class UserBody
    {
        /// <summary>
        /// The name of the user
        /// </summary>
        /// <example>Pete</example>
        [MinLength(2, ErrorMessage = "Name must be at least 2 characters")]
        [MaxLength(30, ErrorMessage = "Name can not be longer than 30 characters")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        /// <summary>
        /// The email of the user
        /// </summary>
        /// <example>pete@mail.com</example>
        [MinLength(3, ErrorMessage = "Please enter a valid email address")]
        [MaxLength(100, ErrorMessage = "Email address can not be longer than 100 characters")]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        /// <summary>
        /// The password of the user
        /// </summary>
        /// <example>Pete</example>
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        [MaxLength(255, ErrorMessage = "Password can not be longer than 255 characters")]
        [DataType(DataType.Text)]
        public string Password { get; set; }

        /// <summary>
        /// The date of birth of the user
        /// </summary>
        /// <example>password</example>
        [DataType(DataType.DateTime, ErrorMessage = "Date of birth must be Date Format")]
        public DateTime? Dateofbirth { get; set; }

        /// <summary>
        /// The gender of the user
        /// </summary>
        /// <example>Male</example>
        [EnumDataType(typeof(UserGender), ErrorMessage = "The gender was not valid")]
        public UserGender? Gender { get; set; }

        /// <summary>
        /// The account type of the user
        /// </summary>
        /// <example>User</example>
        [EnumDataType(typeof(UserType), ErrorMessage = "The user type was not valid")]
        public UserType? Type { get; set; }
    }

    /// <summary>
    /// The gender of the user
    /// </summary>
    public enum UserGender
    {
        /// <summary>
        /// Male
        /// </summary>
        Male,
        /// <summary>
        /// Female
        /// </summary>
        Female,
        /// <summary>
        /// Other
        /// </summary>
        Other
    }

    /// <summary>
    /// The account type of the user
    /// </summary>
    public enum UserType
    {
        /// <summary>
        /// User
        /// </summary>
        User = 0,
        /// <summary>
        /// Organiser
        /// </summary>
        Organiser = 1,
        /// <summary>
        /// Admin
        /// </summary>
        Admin = 2
    }
}