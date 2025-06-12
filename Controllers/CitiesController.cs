using EventPlannerPro.Data;
using EventPlannerPro.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace EventPlannerPro.Controllers
{
    public class CitiesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var cities = await _context.Cities.ToListAsync();
            return View(cities);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var city = await _context.Cities.FindAsync(id);
            if (city == null) return NotFound();

            return View(city);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, City city)
        {
            if (id != city.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(city);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(city);
        }

        public async Task<IActionResult> ByCity(int cityId)
        {
            var activities = await _context.Activities
                .Include(a => a.City)
                .Include(a => a.Category)
                .Where(a => a.CityId == cityId)
                .ToListAsync();

            var city = await _context.Cities.FindAsync(cityId);
            ViewBag.CityName = city?.Name ?? "Unknown";

            return View("ActivitiesByCity", activities);
        }
    }
}
