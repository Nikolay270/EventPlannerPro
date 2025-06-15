using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using EventPlannerPro.Data;

namespace EventPlannerPro.Controllers
{
    [Authorize]
    public class OrganizerController : Controller
    {
        private readonly ApplicationDbContext _context;
        public OrganizerController(ApplicationDbContext ctx) { _context = ctx; }

        public async Task<IActionResult> Index()
        {
            var data = await _context.Activities
                                     .Include(a => a.Organizer)
                                     .GroupBy(a => new { a.OrganizerId, a.Organizer.UserName, a.Organizer.Email })
                                     .Select(g => new
                                     {
                                         g.Key.OrganizerId,
                                         g.Key.UserName,
                                         g.Key.Email,
                                         ActivityCount = g.Count(),
                                         AvgRating = _context.OrganizerReviews
                                                             .Where(r => r.OrganizerId == g.Key.OrganizerId)
                                                             .Select(r => (double?)r.Rating)
                                                             .Average() ?? 0
                                     })
                                     .ToListAsync();
            return View(data);
        }

        public async Task<IActionResult> Details(string id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();

            var activities = await _context.Activities
                                           .Where(a => a.OrganizerId == id)
                                           .ToListAsync();

            var reviews = await _context.OrganizerReviews
                                        .Where(r => r.OrganizerId == id)
                                        .Include(r => r.Reviewer)
                                        .ToListAsync();

            ViewBag.Activities = activities;
            ViewBag.Reviews = reviews;
            ViewBag.Avg = reviews.Any() ? reviews.Average(r => r.Rating) : 0;
            return View(user);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Review(string organizerId, int rating, string comment)
        {
            var reviewerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
            if (organizerId == reviewerId) return RedirectToAction("Details", new { id = organizerId });

            var review = new Models.OrganizerReview
            {
                OrganizerId = organizerId,
                ReviewerId = reviewerId,
                Rating = rating,
                Comment = comment
            };
            _context.OrganizerReviews.Add(review);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = organizerId });
        }
    }
}
