using System.ComponentModel.DataAnnotations;

namespace EventPlannerPro.ViewModels
{
    public class CityCreateViewModel
    {
        [Required(ErrorMessage = "City name is required.")]
        public string Name { get; set; } = string.Empty;
    }
}
