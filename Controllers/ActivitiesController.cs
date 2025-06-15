using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventPlannerPro.Data;
using EventPlannerPro.Models;
using EventPlannerPro.ViewModels;

namespace EventPlannerPro.Controllers
{
    [Authorize]
    public class ActivitiesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ActivitiesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string searchTitle, int? cityId, int? categoryId, string date)
        {
            var query = _context.Activities
                                .Include(a => a.City)
                                .Include(a => a.Category)
                                .AsQueryable();

            var userId = _userManager.GetUserId(User);
            if (!string.IsNullOrEmpty(userId))
                query = query.Where(a => !a.Participants.Any(p => p.UserId == userId));

            if (!string.IsNullOrWhiteSpace(searchTitle))
                query = query.Where(a => a.Title.Contains(searchTitle));

            if (cityId.HasValue)
                query = query.Where(a => a.CityId == cityId);

            if (categoryId.HasValue)
                query = query.Where(a => a.CategoryId == categoryId);

            if (!string.IsNullOrWhiteSpace(date) && DateOnly.TryParse(date, out var d))
                query = query.Where(a => a.StartTime.Date == d.ToDateTime(TimeOnly.MinValue).Date);

            ViewBag.Cities = await _context.Cities.ToListAsync();
            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(await query.ToListAsync());
        }

        public async Task<IActionResult> ByCity(int? cityId)
        {
            if (cityId == null) return NotFound();
            var list = await _context.Activities
                                     .Include(a => a.City)
                                     .Include(a => a.Category)
                                     .Where(a => a.CityId == cityId)
                                     .ToListAsync();
            ViewBag.Cities = await _context.Cities.ToListAsync();
            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View("Index", list);
        }

        public async Task<IActionResult> ByCategory(int? categoryId)
        {
            if (categoryId == null) return NotFound();
            var list = await _context.Activities
                                     .Include(a => a.City)
                                     .Include(a => a.Category)
                                     .Where(a => a.CategoryId == categoryId)
                                     .ToListAsync();
            ViewBag.Cities = await _context.Cities.ToListAsync();
            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View("Index", list);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var activity = await _context.Activities
                                         .Include(a => a.City)
                                         .Include(a => a.Category)
                                         .FirstOrDefaultAsync(a => a.Id == id);
            return activity == null ? NotFound() : View(activity);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Join(int id)
        {
            var userId = _userManager.GetUserId(User);
            var activity = await _context.Activities.Include(a => a.Participants).FirstOrDefaultAsync(a => a.Id == id);
            if (activity == null) return NotFound();

            if (!activity.Participants.Any(p => p.UserId == userId))
            {
                activity.Participants.Add(new ActivityUser { ActivityId = id, UserId = userId });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Joined));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Leave(int id)
        {
            var userId = _userManager.GetUserId(User);
            var activity = await _context.Activities.Include(a => a.Participants).FirstOrDefaultAsync(a => a.Id == id);
            if (activity == null) return NotFound();

            var link = activity.Participants.FirstOrDefault(p => p.UserId == userId);
            if (link != null)
            {
                activity.Participants.Remove(link);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Joined));
        }

        public async Task<IActionResult> Joined()
        {
            var userId = _userManager.GetUserId(User);
            var list = await _context.Activities
                                     .Include(a => a.City)
                                     .Include(a => a.Category)
                                     .Include(a => a.Participants)
                                     .Where(a => a.Participants.Any(p => p.UserId == userId))
                                     .ToListAsync();
            return View("JoinedActivities", list);
        }


        [Authorize]
        public IActionResult Create()
        {
            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Name");
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        [Authorize]
        [HttpPost]
        [IgnoreAntiforgeryToken]               
        public async Task<IActionResult> Create([Bind("Title,Description,Place,StartTime,EndTime,CityId,CategoryId,PhotoUrl")] Activity a)
        {
            if (a.CityId == 0 || a.CategoryId == 0 || string.IsNullOrWhiteSpace(a.Title))
            {
                ModelState.AddModelError("", "Title, City and Category are mandatory.");
                ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Name", a.CityId);
                ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", a.CategoryId);
                return View(a);
            }

            a.OrganizerId = _userManager.GetUserId(User);
            _context.Add(a);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var activity = await _context.Activities.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
            if (activity == null) return NotFound();

            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Name", activity.CityId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", activity.CategoryId);
            return View(activity);
        }

        [Authorize]
        [HttpPost]
        [IgnoreAntiforgeryToken]   // ← no antiforgery cookie required
        public async Task<IActionResult> Edit(Activity a)
        {
            if (a.Id == 0 || string.IsNullOrWhiteSpace(a.Title) || a.CityId == 0 || a.CategoryId == 0)
            {
                ModelState.AddModelError("", "Title, City and Category are mandatory.");
                ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Name", a.CityId);
                ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", a.CategoryId);
                return View(a);
            }

            var original = await _context.Activities.AsNoTracking().FirstOrDefaultAsync(x => x.Id == a.Id);
            if (original == null) return NotFound();

            a.OrganizerId = original.OrganizerId;          
            _context.Update(a);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        [Authorize]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var activity = await _context.Activities.FindAsync(id);
            if (activity == null) return NotFound();

            _context.Activities.Remove(activity);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
