using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillsGardenApi.Models
{
    public class Beacon
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? LocationId { get; set; }

        public string Name { get; set; }

        public Double? Lat { get; set; }

        public Double? Lng { get; set; }

        [JsonIgnore]
        public virtual Location Location { get; set; }

        [JsonIgnore]
        public virtual List<BeaconLog> BeaconLogs { get; set; }
    }
}
