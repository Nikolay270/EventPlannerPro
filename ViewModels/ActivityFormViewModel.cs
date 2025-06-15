using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EventPlannerPro.ViewModels
{
    public class ActivityFormViewModel
    {
        [Required, MaxLength(100)]
        public string Title { get; set; }

        [MaxLength(2000)]
        public string Description { get; set; }

        [Required]
        public string Place { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        [Required]
        public int? CityId { get; set; }

        [Required]
        public int? CategoryId { get; set; }

        public string PhotoUrl { get; set; }

        public SelectList Cities { get; set; }
        public SelectList Categories { get; set; }
    }
}
