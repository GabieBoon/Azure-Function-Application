using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillsGardenApi.Models
{
    public class BeaconLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int64 Id { get; set; }

        public int BeaconId { get; set; }

        public int UserId { get; set; }

        public DateTime Timestamp { get; set; }

        [JsonIgnore]
        public virtual Beacon Beacon { get; set; }
    }
}
