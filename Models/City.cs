
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EventPlannerPro.Models
{
    public class City
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public ICollection<Activity> Activities { get; set; }
    }
}
