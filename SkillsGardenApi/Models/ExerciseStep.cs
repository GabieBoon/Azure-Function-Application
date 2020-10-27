using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillsGardenApi.Models
{
    public class ExerciseStep
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int StepNumber { get; set; }

        public string StepDescription { get; set; }

        [JsonIgnore]
        public virtual Exercise Exercise { get; set; }
    }
}
