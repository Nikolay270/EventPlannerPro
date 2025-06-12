using EventPlannerPro.Data;
using EventPlannerPro.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EventPlannerPro.Controllers
{
    public class ActivitiesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ActivitiesController(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index(string searchTitle, int? cityId, int? categoryId, DateTime? startDate)
        {
            var userId = _userManager.GetUserId(User);

            var joinedActivityIds = await _context.ActivityUsers
                .Where(au => au.UserId == userId)
                .Select(au => au.ActivityId)
                .ToListAsync();

            var query = _context.Activities
                .Where(a => !joinedActivityIds.Contains(a.Id))
                .Include(a => a.City)
                .Include(a => a.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTitle))
            {
                query = query.Where(a => a.Title.Contains(searchTitle));
            }

            if (cityId.HasValue)
            {
                query = query.Where(a => a.CityId == cityId.Value);
            }

            if (categoryId.HasValue)
            {
                query = query.Where(a => a.CategoryId == categoryId.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(a => a.StartTime.Date == startDate.Value.Date);
            }

            // These will be passed using ViewBag for filters
            ViewBag.Cities = new SelectList(await _context.Cities.ToListAsync(), "Id", "Name");
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");

            var activities = await query.ToListAsync();
            return View(activities);
        }

        public async Task<IActionResult> JoinedActivities()
        {
            var userId = _userManager.GetUserId(User);
            var joinedActivityIds = await _context.ActivityUsers
                .Where(au => au.UserId == userId)
                .Select(au => au.ActivityId)
                .ToListAsync();

            var activities = await _context.Activities
                .Where(a => joinedActivityIds.Contains(a.Id))
                .Include(a => a.City)
                .Include(a => a.Category)
                .ToListAsync();

            return View(activities);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Join(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var alreadyJoined = await _context.ActivityUsers
                .AnyAsync(au => au.UserId == userId && au.ActivityId == id);

            if (!alreadyJoined)
            {
                var join = new ActivityUser
                {
                    UserId = userId,
                    ActivityId = id
                };

                _context.ActivityUsers.Add(join);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("JoinedActivities");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Leave(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var activityUser = await _context.ActivityUsers
                .FirstOrDefaultAsync(au => au.UserId == userId && au.ActivityId == id);

            if (activityUser != null)
            {
                _context.ActivityUsers.Remove(activityUser);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }


        public IActionResult Create()
        {
            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Name");
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Activity activity)
        {
            if (ModelState.IsValid)
            {
                activity.CreatorId = _userManager.GetUserId(User);
                if (activity.Description != null)
                {
                    activity.Description = activity.Description.TrimEnd('?', ' ');
                }

                _context.Add(activity);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Name", activity.CityId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", activity.CategoryId);
            return View(activity);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var activity = await _context.Activities
                .Include(a => a.City)
                .Include(a => a.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (activity == null) return NotFound();

            return View(activity);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var activity = await _context.Activities.FindAsync(id);
            if (activity == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && activity.CreatorId != userId)
                return Forbid();

            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Name", activity.CityId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", activity.CategoryId);
            return View(activity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Activity activity)
        {
            if (id != activity.Id) return NotFound();

            var existing = await _context.Activities.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
            var userId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");

            if (existing == null || (!isAdmin && existing.CreatorId != userId))
                return Forbid();

            if (ModelState.IsValid)
            {
                activity.CreatorId = existing.CreatorId;

                if (activity.Description != null)
                {
                    activity.Description = activity.Description.TrimEnd('?', ' ');
                }

                _context.Update(activity);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Name", activity.CityId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", activity.CategoryId);
            return View(activity);
        }


        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var activity = await _context.Activities
                .Include(a => a.City)
                .Include(a => a.Category)
                .FirstOrDefaultAsync(a => a.Id == id);

            var userId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");

            if (activity == null || (!isAdmin && activity.CreatorId != userId))
                return Forbid();

            return View(activity);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var activity = await _context.Activities.FindAsync(id);
            var userId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");

            if (activity == null || (!isAdmin && activity.CreatorId != userId))
                return Forbid();

            _context.Activities.Remove(activity);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> ByCategory(int categoryId)
        {
            var activities = await _context.Activities
                .Include(a => a.Category)
                .Include(a => a.City)
                .Where(a => a.CategoryId == categoryId)
                .ToListAsync();

            var category = await _context.Categories.FindAsync(categoryId);
            ViewBag.CategoryName = category?.Name ?? "Unknown";

            return View("ActivitiesByCategory", activities);
        }
        public async Task<IActionResult> ByCity(int cityId)
        {
            var activities = await _context.Activities
                .Include(a => a.Category)
                .Include(a => a.City)
                .Where(a => a.CityId == cityId)
                .ToListAsync();

            var city = await _context.Cities.FindAsync(cityId);
            ViewBag.CityName = city?.Name ?? "Unknown";

            return View("ActivitiesByCity", activities);
        }

    }
}
