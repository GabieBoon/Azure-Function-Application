using System.ComponentModel.DataAnnotations;

namespace SkillsGardenDTO
{
    /// <summary>
    /// The DTO for beacon
    /// </summary>
    public class BeaconBody
    {
        /// <summary>
        /// The name of the beacon
        /// </summary>
        /// <example>Klimrek beacon</example>
        [MinLength(2, ErrorMessage = "Name must be at least 2 characters")]
        [MaxLength(50, ErrorMessage = "Name can not be longer than 50 characters")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        /// <summary>
        /// The locationId of the Location where the beacon is
        /// </summary>
        /// <example>1</example>
        [DataType(DataType.Text)]
        public int? LocationId { get; set; }

        /// <summary>
        /// The latitude of the location of the beacon
        /// </summary>
        /// <example>40.2021</example>
        [DataType(DataType.Text)]
        public double? Lat { get; set; }

        /// <summary>
        /// The longitude of the location of the Skills Garden
        /// </summary>
        /// <example>2.1234</example>
        [DataType(DataType.Text)]
        public double? Lng { get; set; }
    }
}