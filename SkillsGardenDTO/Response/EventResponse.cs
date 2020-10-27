using System;

namespace SkillsGardenDTO.Response
{
    public class EventResponse
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Organisor { get; set; }
        public string Description { get; set; }
        public DateTime? StartTime { get; set; }
        public string Image { get; set; }
        public int? MaxRegistrations { get; set; }
        public int Registrations { get; set; }
    }
}
