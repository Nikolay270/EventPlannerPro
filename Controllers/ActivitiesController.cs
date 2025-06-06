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

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var joinedActivityIds = await _context.ActivityUsers
                .Where(au => au.UserId == userId)
                .Select(au => au.ActivityId)
                .ToListAsync();

            var activities = await _context.Activities
                .Where(a => !joinedActivityIds.Contains(a.Id))
                .Include(a => a.City)
                .Include(a => a.Category)
                .ToListAsync();

            return View(activities);
        }

        [Authorize]
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

        public async Task<IActionResult> Details(int id)
        {
            var activity = await _context.Activities
                .Include(a => a.City)
                .Include(a => a.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (activity == null)
            {
                return NotFound();
            }

            return View(activity);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["CityId"] = new SelectList(_context.Cities.ToList(), "Id", "Name");
            ViewData["CategoryId"] = new SelectList(_context.Categories.ToList(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Activity activity)
        {
            if (ModelState.IsValid)
            {
                _context.Add(activity);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CityId"] = new SelectList(_context.Cities.ToList(), "Id", "Name", activity.CityId);
            ViewData["CategoryId"] = new SelectList(_context.Categories.ToList(), "Id", "Name", activity.CategoryId);
            return View(activity);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var activity = await _context.Activities.FindAsync(id);
            if (activity == null)
            {
                return NotFound();
            }

            ViewData["CityId"] = new SelectList(_context.Cities.ToList(), "Id", "Name", activity.CityId);
            ViewData["CategoryId"] = new SelectList(_context.Categories.ToList(), "Id", "Name", activity.CategoryId);
            return View(activity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Activity activity)
        {
            if (id != activity.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(activity);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Activities.Any(e => e.Id == activity.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["CityId"] = new SelectList(_context.Cities.ToList(), "Id", "Name", activity.CityId);
            ViewData["CategoryId"] = new SelectList(_context.Categories.ToList(), "Id", "Name", activity.CategoryId);
            return View(activity);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var activity = await _context.Activities.FindAsync(id);
            if (activity == null)
            {
                return NotFound();
            }

            _context.Activities.Remove(activity);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Join(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (!_context.ActivityUsers.Any(au => au.UserId == userId && au.ActivityId == id))
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
    }
}
