using System.Collections.Generic;
using EventPlannerPro.Models;
using Microsoft.AspNetCore.Identity;

namespace EventPlannerPro.ViewModels
{
    public class OrganizerIndexViewModel
    {
        public List<Activity> MyActivities { get; set; } = new();
        public List<IdentityUser> Organizers { get; set; } = new();
    }
}