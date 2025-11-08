using AmongClass.Data;
using AmongClass.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AmongClass.Controllers
{
    public class AnswersController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<IdentityUser> _userManager;

        public AnswersController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            db = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var answers = db.Answers
                .Include(a => a.Question)
                .Include(a => a.Votes);
            ViewBag.Answers = answers;
            return View();
        }

        [Authorize]
        public ActionResult New(Guid questionId)
        {
            ViewBag.QuestionId = questionId;
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> New(Answer answer)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    answer.UserId = Guid.Parse(currentUser.Id);
                    db.Answers.Add(answer);
                    db.SaveChanges();
                    return RedirectToAction("Show", "Questions", new { id = answer.QuestionId });
                }
                return Unauthorized();
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

        // de pe claim-uri verifici user-id ul de pe claim si cu user-id ul de la cine a crea answer-ul
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