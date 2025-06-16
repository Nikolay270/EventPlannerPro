using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventPlannerPro.Data;
using EventPlannerPro.Models;
using EventPlannerPro.ViewModels;   // ← contains OrganizerIndexViewModel

namespace EventPlannerPro.Controllers
{
    [Authorize]
    [Route("Organizer")]
    [Route("Organizers")]
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

        // --------------------------------------------------------------------
        // LIST  /Organizer
        // --------------------------------------------------------------------
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var me = _users.GetUserId(User);

            var myActivities = await _ctx.Activities
                                         .Where(a => a.OrganizerId == me)
                                         .Include(a => a.City)
                                         .Include(a => a.Category)
                                         .Include(a => a.Participants)
                                         .ToListAsync();

            var organizers = await _users.GetUsersInRoleAsync("Organizer");

            // ⭐ build average-rating lookup without server-side GroupBy
            var ratingLookup = (await _ctx.OrganizerReviews
                                         .Select(r => new { r.OrganizerId, r.Rating })
                                         .ToListAsync())
                               .GroupBy(x => x.OrganizerId)
                               .ToDictionary(g => g.Key,
                                             g => g.Average(x => x.Rating));

            ViewBag.Ratings = ratingLookup;

            return View("Index", new OrganizerIndexViewModel      // ← existing ViewModel
            {
                MyActivities = myActivities,
                Organizers = organizers.ToList()
            });
        }

        // --------------------------------------------------------------------
        // PROFILE  /Organizer/Profile/{id}
        // --------------------------------------------------------------------
        [HttpGet("Profile/{id}")]
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

            return View("Profile", org);   // sends IdentityUser + ViewBag data
        }

        // --------------------------------------------------------------------
        // POST  /Organizer/Review
        // --------------------------------------------------------------------
        [HttpPost("Review"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Review(string organizerId, int rating, string comment)
        {
            if (rating < 1 || rating > 5) return BadRequest("Rating must be 1-5.");

            var me = _users.GetUserId(User);
            if (me == organizerId)
            {
                TempData["Error"] = "You can’t review yourself.";
                return RedirectToAction("Profile", new { id = organizerId });
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
            return RedirectToAction("Profile", new { id = organizerId });
        }
    }
}
