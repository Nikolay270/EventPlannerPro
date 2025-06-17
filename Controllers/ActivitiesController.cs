using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventPlannerPro.Data;
using EventPlannerPro.Models;

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
            string? userId = _userManager.GetUserId(User);

            var query = _context.Activities
                                .Include(a => a.City)
                                .Include(a => a.Category)
                                .Include(a => a.Participants)
                                .Where(a => a.Participants.Count < a.Capacity)
                                .AsQueryable();


            if (!string.IsNullOrEmpty(userId))
                query = query.Where(a => !a.Participants.Any(p => p.UserId == userId));

            if (!string.IsNullOrWhiteSpace(searchTitle))
                query = query.Where(a => a.Title.Contains(searchTitle));

            if (cityId.HasValue)
                query = query.Where(a => a.CityId == cityId);

            if (categoryId.HasValue)
                query = query.Where(a => a.CategoryId == categoryId);

            if (DateTime.TryParse(date, out var parsed))
                query = query.Where(a => a.StartTime.Date == parsed.Date);

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
                                     .Include(a => a.Participants)
                                     .Where(a => a.CityId == cityId && a.Participants.Count < a.Capacity)
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
                                     .Include(a => a.Participants)
                                     .Where(a => a.CategoryId == categoryId && a.Participants.Count < a.Capacity)
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
                                         .Include(a => a.Participants)
                                         .ThenInclude(p => p.User)
                                         .FirstOrDefaultAsync(a => a.Id == id);
            var me = _userManager.GetUserId(User);

            ViewBag.CanEdit = User.IsInRole("Admin") || activity.OrganizerId == me;
            return activity == null ? NotFound() : View(activity);

        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Join(int id)
        {
            string? userId = _userManager.GetUserId(User);
            var activity = await _context.Activities
                                         .Include(a => a.Participants)
                                         .FirstOrDefaultAsync(a => a.Id == id);

            if (activity == null) return NotFound();

            if (activity.Participants.Count >= activity.Capacity)
            {
                TempData["Error"] = "This activity is already full.";
                return RedirectToAction(nameof(Index));
            }

            if (!activity.Participants.Any(p => p.UserId == userId))
            {
                activity.Participants.Add(new ActivityUser
                {
                    UserId = userId!,
                   
                });
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Joined));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Leave(int id)
        {
            string? userId = _userManager.GetUserId(User);
            var activity = await _context.Activities
                                         .Include(a => a.Participants)
                                         .FirstOrDefaultAsync(a => a.Id == id);
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
            string? userId = _userManager.GetUserId(User);
            var list = await _context.Activities
                                     .Include(a => a.City)
                                     .Include(a => a.Category)
                                     .Include(a => a.Participants)
                                     .Where(a => a.Participants.Any(p => p.UserId == userId))
                                     .ToListAsync();
            return View("JoinedActivities", list);
        }

        public IActionResult Create()
        {
            ViewBag.Cities = _context.Cities.ToList();
            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSimple(
            string title,
            string description,
            string photoUrl,
            string startTime,
            int? cityId,
            int? categoryId,
            int? capacity)
        {
           
            DateTime parsedStart = DateTime.TryParse(startTime, out var dt)
                                   ? dt
                                   : DateTime.Now;

            var activity = new EventPlannerPro.Models.Activity
            {
                Title = title,
                Description = description,
                PhotoUrl = photoUrl ?? "",
                StartTime = parsedStart,
                Capacity = capacity ?? 0,
                CityId = cityId ?? 0,
                CategoryId = categoryId ?? 0,
                OrganizerId = _userManager.GetUserId(User)!
            };

   
            _context.Add(activity);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            
            var activity = await _context.Activities.FindAsync(id);
            if (activity == null) return NotFound();

            ViewBag.Cities = _context.Cities.ToList();
            ViewBag.Categories = _context.Categories.ToList();

            return View(activity);
        }

       
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(
            int id,
            string title,
            string description,
            string photoUrl,
            string startTime,
            int? cityId,
            int? categoryId,
            int? capacity)
        {
            
            var activity = await _context.Activities.FindAsync(id);
            if (activity == null) return NotFound();

           
            activity.Title = title;
            activity.Description = description;
            activity.PhotoUrl = photoUrl ?? "";
            activity.StartTime = DateTime.TryParse(startTime, out var dt) ? dt : activity.StartTime;
            activity.Capacity = capacity ?? activity.Capacity;
            activity.CityId = cityId ?? activity.CityId;
            activity.CategoryId = categoryId ?? activity.CategoryId;
      
            _context.Update(activity);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var activity = await _context.Activities.FindAsync(id);
            if (activity == null) return NotFound();


            if (activity.OrganizerId != _userManager.GetUserId(User) && !User.IsInRole("Admin"))
                return Forbid();

            _context.Activities.Remove(activity);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
