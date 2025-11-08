using AmongClass.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AmongClass.Controllers
{
    public class SessionsController(ApplicationDbContext context) : Controller
    {
        private readonly ApplicationDbContext db = context;
        public IActionResult Index()
        {
            var sessions = db.Sessions.Include(s => s.Scores).Include(c => c.Teacher);

            ViewBag.Sessions = sessions;

            return View();
        }
    }
}
