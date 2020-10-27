using SkillsGardenApi.Models;
using System;

namespace SkillsGardenApiTests.Factory
{
    public static class EventFactory
    {
        public static Event CreateEvent(int id, int organisorId, int locationId)
        {
            return new Event
            {
                Id = id,
                Title = "Skillgarden Amsterdam",
                OrganisorId = organisorId,
                Description = $"Test Event {organisorId} description",
                LocationId = locationId,
                StartTime = DateTime.Now.AddDays(1),
                Image = "da82f586-1bd8-4da4-9813-a1a87f54fb51.png",
                EventRegistrations = null
            };
        }
    }
}
