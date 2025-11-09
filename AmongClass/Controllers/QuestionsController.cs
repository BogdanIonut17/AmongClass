using AmongClass.Data;
using AmongClass.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AmongClass.Controllers
{
    public class QuestionsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<IdentityUser> _userManager;

        public QuestionsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            db = context;
            _userManager = userManager;
        }

        public ActionResult Index()
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

                //return RedirectToAction("Index");

                return RedirectToAction("Ask", "Ollama");
            }
            catch (Exception e)
            {
                return View();
            }
        }

        public async Task<IActionResult> Show(Guid id)
        {
            Question q = db.Questions
                            .Include(a => a.Answers)
                                .ThenInclude(a => a.Votes)
                            .Where(a => a.Id == id)
                            .FirstOrDefault();

            if (q == null)
            {
                return NotFound();
            }

            ViewBag.Question = q;
            ViewBag.Answers = q.Answers;

            if (User.Identity.IsAuthenticated)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    var userVote = db.Votes
                        .Include(v => v.Answer)
                        .Where(v => v.UserId == currentUser.Id && v.Answer.QuestionId == id)
                        .FirstOrDefault();

                    ViewBag.UserVote = userVote;
                }
            }

            return View();
        }

        public ActionResult Edit(Guid id)
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
                if (q != null)
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
        [ValidateAntiForgeryToken]
        public ActionResult Delete(Guid id)
        {
            // ✅ SOLUȚIE: Încarcă întrebarea CU toate răspunsurile și voturile
            Question q = db.Questions
                .Include(q => q.Answers)
                    .ThenInclude(a => a.Votes)  // Include și voturile
                .Include(q => q.SessionQuestions)  // Include și legăturile cu sesiuni
                .FirstOrDefault(q => q.Id == id);

            if (q == null)
            {
                return NotFound();
            }

            // ✅ Șterge mai întâi voturile pentru fiecare răspuns
            foreach (var answer in q.Answers.ToList())
            {
                // Șterge voturile
                db.Votes.RemoveRange(answer.Votes);
            }

            // ✅ Șterge răspunsurile
            db.Answers.RemoveRange(q.Answers);

            // ✅ Șterge legăturile cu sesiunile (dacă există)
            if (q.SessionQuestions != null && q.SessionQuestions.Any())
            {
                db.SessionQuestions.RemoveRange(q.SessionQuestions);
            }

            // ✅ Acum șterge întrebarea
            db.Questions.Remove(q);

            db.SaveChanges();

            TempData["SuccessMessage"] = "Întrebarea a fost ștearsă cu succes!";
            return RedirectToAction("Index");
        }
    }
}