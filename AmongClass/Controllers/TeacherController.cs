using Microsoft.AspNetCore.Mvc;

namespace AmongClass.Controllers
{
    public class TeacherController : Controller
    {
        private readonly IRepository _repository; // sau contextul tău de date

        public TeacherController(IRepository repository)
        {
            _repository = repository;
        }

        public IActionResult Dashboard()
        {
            var sessions = _repository.GetAllSessions(); // adaptează la metoda ta
            return View(sessions);
        }
    }
}
