using System.Collections.Generic;

namespace SkillsGardenDTO.Response
{
    public class WorkoutResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public List<ExerciseResponse> Exercises { get; set; }
    }
}
