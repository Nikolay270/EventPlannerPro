using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace EventPlannerPro.Models
{
    public class Activity
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Title { get; set; }

        public string Description { get; set; }

        [Display(Name = "Photo URL")]
        public string PhotoUrl { get; set; }

        [Display(Name = "City")]
        public int CityId { get; set; }
        public City City { get; set; }

        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        [Display(Name = "Start Time")]
        public DateTime StartTime { get; set; }

        [Display(Name = "Capacity")]
        public int Capacity { get; set; }

        public ICollection<ActivityUser> Participants { get; set; } = new List<ActivityUser>();

        // Track the creator
        [ValidateNever]
        [ForeignKey("Organizer")]

       
        public string OrganizerId { get; set; }
        public IdentityUser Organizer { get; set; }
    }
}
