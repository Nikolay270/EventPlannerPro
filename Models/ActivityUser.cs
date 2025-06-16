using System;
using Microsoft.AspNetCore.Identity;

namespace EventPlannerPro.Models
{
    public class ActivityUser
    {
        public string UserId { get; set; } = string.Empty;
        public IdentityUser User { get; set; } = null!;

        public int ActivityId { get; set; }
        public Activity Activity { get; set; } = null!;

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}