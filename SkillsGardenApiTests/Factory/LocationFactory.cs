using SkillsGardenApi.Models;
using SkillsGardenApi.Utils;
using SkillsGardenDTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkillsGardenApiTests.Factory
{
    public static class LocationFactory
    {
        public static Location CreateLocation(int id = 1)
        {
            return new Location
            {
                Id = id,
                Name = "Skillgarden Amsterdam",
                City = "Amsterdam",
                Lat = 5.2379082,
                Lng = 4.899964,
                Image = "445d043a-2e84-4a9b-8a96-036ee0b01e64.png"
            };
        }
    }
}
