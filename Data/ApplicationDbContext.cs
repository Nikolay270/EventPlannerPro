using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EventPlannerPro.Models;

namespace EventPlannerPro.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Activity> Activities { get; set; } = null!;
        public DbSet<ActivityUser> ActivityUsers { get; set; } = null!;
        public DbSet<City> Cities { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<OrganizerReview> OrganizerReviews { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ActivityUser>()
                   .HasKey(au => new { au.UserId, au.ActivityId });

            builder.Entity<ActivityUser>()
                   .HasOne(au => au.User)
                   .WithMany()
                   .HasForeignKey(au => au.UserId);

            builder.Entity<ActivityUser>()
                   .HasOne(au => au.Activity)
                   .WithMany(a => a.Participants)
                   .HasForeignKey(au => au.ActivityId);

            builder.Entity<Activity>(e =>
            {
                e.HasOne(a => a.Organizer)
                 .WithMany()
                 .HasForeignKey(a => a.OrganizerId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<OrganizerReview>(e =>
            {
                e.HasOne(r => r.Organizer)
                 .WithMany()
                 .HasForeignKey(r => r.OrganizerId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(r => r.Reviewer)
                 .WithMany()
                 .HasForeignKey(r => r.ReviewerId)
                 .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
