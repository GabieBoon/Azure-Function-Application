using Newtonsoft.Json;
using SkillsGardenDTO;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillsGardenApi.Models
{
    public class ExerciseForm
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public MovementForm MovementForm { get; set; }

        [JsonIgnore]
        public virtual Exercise Exercise { get; set; }
    }
}
