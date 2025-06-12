using System.ComponentModel.DataAnnotations;

namespace EventPlannerPro.Models
{
    public class City
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Display(Name = "Photo URL")]
        public string? PhotoUrl { get; set; }

        public ICollection<Activity>? Activities { get; set; }
    }
}
