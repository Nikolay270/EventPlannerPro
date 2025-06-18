using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace EventPlannerPro.Models
{
    public class ActivityUser
    {
        [Key]
        public int Id { get; set; }

        // FK back to Activity
        public int ActivityId { get; set; }
        [ForeignKey("ActivityId")]
        public Activity Activity { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public IdentityUser User { get; set; }
    }
}
