using AmongClass.Data;
using AmongClass.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AmongClass.Controllers
{
    public class QuestionsController(ApplicationDbContext context) : Controller
    {
        private readonly ApplicationDbContext db = context;
        public IActionResult Index()
        {
            var questions = db.Questions.Include(q => q.Answers);

            ViewBag.Questions = questions;

            return View();
        }
        public ActionResult New()
        {
            return View();
        }

        [HttpPost]
        public ActionResult New(Question q)
        {
            try
            {
                db.Questions.Add(q);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View();
            }
        }
        public IActionResult Show(Guid id)
        {
            Question q = db.Questions
                            .Include(a => a.Answers)
                            .Where(a => a.Id == id)
                            .First();

            if (q == null)
            {
                return NotFound();
            }

            ViewBag.Article = q;

            ViewBag.Answers = q.Answers;

            return View();
        }
        public ActionResult Edit(int id)
        {
            Question q = db.Questions.Find(id);
            ViewBag.Question = q;
            return View();
        }
        [HttpPost]
        public IActionResult Edit(Guid id, Question requestQuestion)
        {
            try
            {
                Question q = db.Questions.Find(id);

                {
                    q.Text = requestQuestion.Text;
                    db.SaveChanges();
                }

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                ViewBag.Question = requestQuestion;
                return View();
            }
        }
        [HttpPost]
        public ActionResult Delete(int id)
        {
            Question q = db.Questions.Find(id);
            db.Questions.Remove(q);
            db.SaveChanges();
            return RedirectToAction("Index");
        }



    }
}
