using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace EventPlannerPro.Models
{
    public class Activity
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Title { get; set; }

        public string Description { get; set; } = string.Empty;

        public string Place { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        [Required]
        public int CityId { get; set; }
        public City City { get; set; }

        [Required]
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public string PhotoUrl { get; set; } = string.Empty;

        public string? OrganizerId { get; set; }
        public IdentityUser? Organizer { get; set; }

        [NotMapped]                 
        public string? CreatorId
        {
            get => OrganizerId;
            set => OrganizerId = value;
        }

        public List<ActivityUser> Participants { get; set; } = new();
    }
}
