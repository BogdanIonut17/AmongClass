using AmongClass.Data;
using AmongClass.Helpers;
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
        private readonly RagService _rag;
        private readonly UserManager<IdentityUser> _userManager;

        public AnswersController(ApplicationDbContext context, UserManager<IdentityUser> userManager, RagService rag)
        {
            db = context;
            _userManager = userManager;
            _rag = rag;
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
            if (!ModelState.IsValid) return View(answer);

            answer.Id = Guid.NewGuid();
            answer.UserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;
            db.Answers.Add(answer);
            await db.SaveChangesAsync();

            // Generează răspuns AI în fundal
            var questionId = answer.QuestionId;
            _ = Task.Run(async () =>
            {
                try
                {
                    var question = await db.Questions.FindAsync(questionId);
                    if (question != null)
                    {
                        string aiResponseText = await _rag.GetRelevantRules(question.Text);
                        var aiAnswer = new Answer
                        {
                            Id = Guid.NewGuid(),
                            Text = aiResponseText,
                            QuestionId = questionId,
                            UserId = Guid.Empty.ToString()
                        };
                        db.Answers.Add(aiAnswer);
                        await db.SaveChangesAsync();
                    }
                }
                catch { }
            });

            return RedirectToAction("Show", "Questions", new { id = answer.QuestionId });
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