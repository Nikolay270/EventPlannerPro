using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace EventPlannerPro.Models
{
    public class OrganizerReview
    {
        public int Id { get; set; }

        [Required]
        public string ReviewerId { get; set; }

        [Required]
        public string OrganizerId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(500)]
        public string Comment { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public IdentityUser Reviewer { get; set; }
        public IdentityUser Organizer { get; set; }
    }
}
