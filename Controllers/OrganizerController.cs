using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventPlannerPro.Data;
using EventPlannerPro.Models;
using EventPlannerPro.ViewModels;

namespace EventPlannerPro.Controllers
{
    [Authorize]
    public class OrganizerController : Controller
    {
        private readonly ApplicationDbContext _ctx;
        private readonly UserManager<IdentityUser> _users;

        public OrganizerController(ApplicationDbContext ctx,
                                   UserManager<IdentityUser> users)
        {
            _ctx = ctx;
            _users = users;
        }

        // GET /Organizer or /Organizer/Index
        public async Task<IActionResult> Index()
        {
            var me = _users.GetUserId(User);

            // Activities *you* organized
            var myActivities = await _ctx.Activities
                                         .Where(a => a.OrganizerId == me)
                                         .Include(a => a.City)
                                         .Include(a => a.Category)
                                         .Include(a => a.Participants)
                                         .ToListAsync();

            // All distinct organizer‐IDs who have at least one activity,
            // excluding the current user (so you don’t see yourself in "All Organizers")
            var organizerIds = await _ctx.Activities
                                         .Select(a => a.OrganizerId)
                                         .Where(id => id != me)
                                         .Distinct()
                                         .ToListAsync();

            // Load only those users
            var organizers = await _users.Users
                                         .Where(u => organizerIds.Contains(u.Id))
                                         .ToListAsync();

            // Build average‐rating lookup
            var ratingLookup = (await _ctx.OrganizerReviews
                                         .Select(r => new { r.OrganizerId, r.Rating })
                                         .ToListAsync())
                               .GroupBy(x => x.OrganizerId)
                               .ToDictionary(
                                   g => g.Key,
                                   g => Math.Round(g.Average(x => x.Rating), 1)
                               );

            ViewBag.Ratings = ratingLookup;

            var vm = new OrganizerIndexViewModel
            {
                MyActivities = myActivities,
                Organizers = organizers
            };
            return View(vm);
        }

        // GET /Organizer/Profile/{id}
        [HttpGet]
        public async Task<IActionResult> Profile(string id)
        {
            var org = await _users.FindByIdAsync(id);
            if (org == null) return NotFound();

            var acts = await _ctx.Activities
                                 .Where(a => a.OrganizerId == id)
                                 .Include(a => a.City)
                                 .Include(a => a.Category)
                                 .ToListAsync();

            var reviews = await _ctx.OrganizerReviews
                                    .Where(r => r.OrganizerId == id)
                                    .Include(r => r.Reviewer)
                                    .OrderByDescending(r => r.CreatedOn)
                                    .ToListAsync();

            ViewBag.Activities = acts;
            ViewBag.Reviews = reviews;
            ViewBag.Average = reviews.Any() ? reviews.Average(r => r.Rating) : 0.0;

            if (User.Identity!.IsAuthenticated)
            {
                var me = _users.GetUserId(User);
                ViewBag.MyReview = reviews.FirstOrDefault(r => r.ReviewerId == me);
            }

            return View(org);
        }

        // POST /Organizer/Review
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Review(string organizerId, int rating, string comment)
        {
            if (rating < 1 || rating > 5) return BadRequest("Rating must be 1-5.");
            var me = _users.GetUserId(User);
            if (me == organizerId)
            {
                TempData["Error"] = "You can’t review yourself.";
                return RedirectToAction(nameof(Profile), new { id = organizerId });
            }

            var rec = await _ctx.OrganizerReviews
                                .FirstOrDefaultAsync(r => r.OrganizerId == organizerId &&
                                                          r.ReviewerId == me);

            if (rec == null)
            {
                _ctx.OrganizerReviews.Add(new OrganizerReview
                {
                    OrganizerId = organizerId,
                    ReviewerId = me,
                    Rating = rating,
                    Comment = comment?.Trim() ?? "",
                    CreatedOn = DateTime.UtcNow
                });
            }
            else
            {
                rec.Rating = rating;
                rec.Comment = comment?.Trim() ?? "";
                rec.CreatedOn = DateTime.UtcNow;
            }

            await _ctx.SaveChangesAsync();
            return RedirectToAction(nameof(Profile), new { id = organizerId });
        }
    }
}
