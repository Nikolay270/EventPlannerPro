using System.Collections.Generic;
using EventPlannerPro.Models;
using Microsoft.AspNetCore.Identity;

namespace EventPlannerPro.ViewModels
{
    public class OrganizerIndexViewModel
    {
        public IEnumerable<Activity> MyActivities { get; set; } = new List<Activity>();
        public IEnumerable<IdentityUser> Organizers { get; set; } = new List<IdentityUser>();
       
    }
}