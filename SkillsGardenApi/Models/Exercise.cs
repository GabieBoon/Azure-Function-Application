using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillsGardenApi.Models
{
    public class Exercise
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Media { get; set; }

        public virtual ICollection<ExerciseRequirement> ExerciseRequirements { get; set; }

        public virtual ICollection<ExerciseStep> ExerciseSteps { get; set; }

        public virtual ICollection<ExerciseForm> ExerciseForms { get; set; }
    }
}
