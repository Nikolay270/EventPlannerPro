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
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Place { get; set; } = string.Empty;

        [DataType(DataType.DateTime)]
        public DateTime StartTime { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime EndTime { get; set; }

        public string? PhotoUrl { get; set; }

        // FK
        public int CityId { get; set; }
        public City? City { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public string? OrganizerId { get; set; }
        public IdentityUser? Organizer { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be at least 1.")]
        public int Capacity { get; set; }

        [NotMapped]
        public string? CreatorId
        {
            get => OrganizerId;
            set => OrganizerId = value;
        }

        public List<ActivityUser> Participants { get; set; } = new();
    }
}