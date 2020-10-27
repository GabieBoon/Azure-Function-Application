using System.Collections.Generic;

namespace SkillsGardenDTO.Response
{
    public class ComponentResponse
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Image { get; set; }

        public List<int> Exercises { get; set; }
    }
}
