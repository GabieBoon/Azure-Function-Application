using SkillsGardenApi.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkillsGardenApiTests.Factory
{
    class ComponentFactory
    {
        public static Component CreateComponent(int componentId, Location location)
        {
            return new Component
            {
                Id = componentId,
                Name = "Klimrek",
                Description = "Je kunt diverse activiteiten doen op dit geweldige kleurijke klimrek",
                Image = "445d043a-2e84-4a9b-8a96-036ee0b01e64.png",
                Location = location
            };
        }
    }
}
