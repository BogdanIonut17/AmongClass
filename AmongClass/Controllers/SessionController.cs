using Microsoft.AspNetCore.Mvc;

namespace AmongClass.Controllers
{
    public class SessionController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
