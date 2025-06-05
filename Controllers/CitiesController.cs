using Microsoft.AspNetCore.Mvc;
using EventPlannerPro.Data;
using EventPlannerPro.Models;
using EventPlannerPro.ViewModels;

namespace EventPlannerPro.Controllers
{
    public class CitiesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View(_context.Cities.ToList());
        }

        public IActionResult Create()
        {
            return View(new CityCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CityCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var city = new City { Name = model.Name.Trim() };
            _context.Cities.Add(city);
            _context.SaveChanges();

            TempData["Message"] = $"City '{city.Name}' created successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
