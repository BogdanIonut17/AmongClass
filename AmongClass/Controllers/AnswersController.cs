using AmongClass.Data;
using AmongClass.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AmongClass.Controllers
{
    public class AnswersController(ApplicationDbContext context) : Controller
    {
        private readonly ApplicationDbContext db = context;

        public IActionResult Index()
        {
            // Include Question pentru a afișa întrebarea asociată fiecărui răspuns
            var answers = db.Answers.Include(a => a.Question);
            ViewBag.Answers = answers;
            return View();
        }

        public ActionResult New(Guid questionId)
        {
            ViewBag.QuestionId = questionId;
            return View();
        }

        [HttpPost]
        public ActionResult New(Answer answer)
        {
            try
            {
                db.Answers.Add(answer);
                db.SaveChanges();
                return RedirectToAction("Show", "Questions", new { id = answer.QuestionId });
            }
            catch (Exception e)
            {
                ViewBag.QuestionId = answer.QuestionId;
                return View();
            }
        }

        public ActionResult Edit(Guid id)
        {
            Answer answer = db.Answers
                              .Include(a => a.Question)
                              .Where(a => a.Id == id)
                              .FirstOrDefault();

            if (answer == null)
            {
                return NotFound();
            }

            ViewBag.Answer = answer;
            return View();
        }

        [HttpPost]
        public IActionResult Edit(Guid id, Answer requestAnswer)
        {
            try
            {
                Answer answer = db.Answers.Find(id);

                if (answer == null)
                {
                    return NotFound();
                }

                answer.Text = requestAnswer.Text;
                db.SaveChanges();

                return RedirectToAction("Show", "Questions", new { id = answer.QuestionId });
            }
            catch (Exception e)
            {
                ViewBag.Answer = requestAnswer;
                return View();
            }
        }

        [HttpPost]
        public ActionResult Delete(Guid id)
        {
            Answer answer = db.Answers.Find(id);

            if (answer == null)
            {
                return NotFound();
            }

            Guid questionId = answer.QuestionId;
            db.Answers.Remove(answer);
            db.SaveChanges();

            return RedirectToAction("Show", "Questions", new { id = questionId });
        }
    }
}