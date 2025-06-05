using Microsoft.AspNetCore.Mvc;

namespace EventPlannerPro.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
