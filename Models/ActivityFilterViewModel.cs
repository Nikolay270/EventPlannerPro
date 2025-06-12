using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace EventPlannerPro.Models
{
    public class ActivityFilterViewModel
    {
        public string? SearchTitle { get; set; }
        public int? CityId { get; set; }
        public int? CategoryId { get; set; }
        public DateTime? StartDate { get; set; }

        public List<SelectListItem>? Cities { get; set; }
        public List<SelectListItem>? Categories { get; set; }

        public IEnumerable<Activity> Activities { get; set; } = new List<Activity>();
    }
}
