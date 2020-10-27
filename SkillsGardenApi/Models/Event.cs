using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillsGardenApi.Models
{
    public class Event
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Title { get; set; }

        public int OrganisorId { get; set; }

        public string Description { get; set; }

        public DateTime? StartTime { get; set; }

        public string Image { get; set; }

        public int? MaxRegistrations { get; set; }

        public int LocationId { get; set; }

        [JsonIgnore]
        public virtual ICollection<Registration> EventRegistrations { get; set; }

        [JsonIgnore]
        public virtual Location Location { get; set; }
    }
}
