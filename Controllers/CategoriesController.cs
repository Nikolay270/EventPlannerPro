using Microsoft.AspNetCore.Mvc;
using EventPlannerPro.Data;
using EventPlannerPro.Models;
using EventPlannerPro.ViewModels;

namespace EventPlannerPro.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View(_context.Categories.ToList());
        }

        public IActionResult Create()
        {
            return View(new CategoryCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CategoryCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var category = new Category { Name = model.Name.Trim() };
            _context.Categories.Add(category);
            _context.SaveChanges();

            TempData["Message"] = $"Category '{category.Name}' created successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
