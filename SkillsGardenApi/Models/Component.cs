using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillsGardenApi.Models
{
    public class Component
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Image { get; set; }

        [JsonIgnore]
        public int LocationId { get; set; }

        [JsonIgnore]
        public virtual ICollection<ComponentExercise> ComponentExercises { get; set; }

        [JsonIgnore]
        public virtual Location Location { get; set; }

    }
}
