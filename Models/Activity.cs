using System.ComponentModel.DataAnnotations;

namespace EventPlannerPro.Models
{
    public class Activity
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Place { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Start Time")]
        public DateTime StartTime { get; set; }

        [Required]
        [Display(Name = "End Time")]
        public DateTime EndTime { get; set; }

        [Required]
        [Display(Name = "City")]
        public int CityId { get; set; }
        public City? City { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        [Display(Name = "Created By")]
        public string? CreatorId { get; set; } 

        public string? Description { get; set; }

        public ICollection<ActivityUser> Participants { get; set; } = new List<ActivityUser>();

        [Display(Name = "Photo URL")]
        [Url]
        public string? PhotoUrl { get; set; }

    }
}
