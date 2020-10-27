using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillsGardenApi.Models
{
    public class Location
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }

        public string City { get; set; }

        public Double? Lat { get; set; }

        public Double? Lng { get; set; }

        public string Image { get; set; }

        [JsonIgnore]
        public virtual ICollection<Component> Components { get; set; }

        [JsonIgnore]
        public virtual ICollection<Event> Events { get; set; }
    }
}
