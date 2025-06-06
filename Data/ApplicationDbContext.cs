
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EventPlannerPro.Models;

namespace EventPlannerPro.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Activity> Activities { get; set; } = null!;
        public DbSet<ActivityUser> ActivityUsers { get; set; } = null!;
        public DbSet<City> Cities { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Roles
            builder.Entity<IdentityRole>().HasData(
                new IdentityRole { Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole { Name = "User", NormalizedName = "USER" }
            );

            // Cities
            builder.Entity<City>().HasData(
                new City { Id = 1, Name = "Sofia" },
                new City { Id = 2, Name = "Plovdiv" },
                new City { Id = 3, Name = "Varna" }
            );

            // Categories
            builder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Concert" },
                new Category { Id = 2, Name = "Festival" },
                new Category { Id = 3, Name = "Workshop" }
            );

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
        }
    }
}
