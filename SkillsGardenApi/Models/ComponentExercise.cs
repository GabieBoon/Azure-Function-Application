using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillsGardenApi.Models
{
    public class ComponentExercise
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ComponentId { get; set; }

        public int ExerciseId { get; set; }

        [JsonIgnore]
        public virtual Component Component { get; set; }
    }
}
