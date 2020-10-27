using System.Collections.Generic;

namespace SkillsGardenDTO.Response
{
    public class ExerciseResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Media { get; set; }
        public List<string> Requirements { get; set; }
        public List<string> Steps { get; set; }
        public List<MovementForm> Forms { get; set; }
    }
}
