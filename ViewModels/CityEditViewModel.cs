using System.ComponentModel.DataAnnotations;

namespace EventPlannerPro.ViewModels
{
    public class CityEditViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [Display(Name = "Photo URL")]
        public string? PhotoUrl { get; set; }
    }
}
