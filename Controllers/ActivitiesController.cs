using Microsoft.AspNetCore.Authorization;
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

        public ActivitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Activities/Create
        public IActionResult Create()
        {
            PopulateDropdowns();
            return View();
        }

        // POST: Activities/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ActivityCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                PopulateDropdowns();
                return View(vm);
            }

            var activity = new Activity
            {
                Title = vm.Title,
                Place = vm.Place,
                StartTime = vm.StartTime,
                EndTime = vm.EndTime,
                CityId = vm.CityId,
                CategoryId = vm.CategoryId,
                CreatedById = User.Identity?.Name,
                Description = vm.Description,

            };

            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Activity created successfully!";
            return RedirectToAction(nameof(Index));
        }

        private void PopulateDropdowns()
        {
            ViewBag.Cities = new SelectList(_context.Cities, "Id", "Name");
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var list = await _context.Activities
                .Include(a => a.City)
                .Include(a => a.Category)
                .ToListAsync();

            return View(list);
        }
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var activity = await _context.Activities
                .Include(a => a.City)
                .Include(a => a.Category)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (activity == null)
                return NotFound();

            return View(activity);
        }

    }
}
