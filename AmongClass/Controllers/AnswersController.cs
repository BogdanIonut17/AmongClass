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

        // GUID constant pentru AI - va fi același în toată aplicația
        public static readonly string AI_USER_ID = "11111111-1111-1111-1111-111111111111";

        public AnswersController(ApplicationDbContext context, UserManager<IdentityUser> userManager, RagService rag)
        {
            db = context;
            _userManager = userManager;
            _rag = rag;

            // Asigură-te că user-ul AI există în baza de date
            EnsureAiUserExists().Wait();
        }

        private async Task EnsureAiUserExists()
        {
            var aiUser = await _userManager.FindByIdAsync(AI_USER_ID);

            if (aiUser == null)
            {
                // Creează user-ul AI
                aiUser = new IdentityUser
                {
                    Id = AI_USER_ID,
                    UserName = "AI_Assistant",
                    Email = "ai@amongclass.system",
                    EmailConfirmed = true,
                    LockoutEnabled = false,
                    // Nu poate face login - nu are parolă
                };

                var result = await _userManager.CreateAsync(aiUser);

                if (result.Succeeded)
                {
                    Console.WriteLine("✓ AI User created successfully");
                }
            }
        }

        public IActionResult Index()
        {
            var answers = db.Answers
                .Include(a => a.Question)
                .Include(a => a.Votes)
                .Where(a => a.UserId != AI_USER_ID); // Excludem răspunsurile AI
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
                            UserId = AI_USER_ID
                        };
                        db.Answers.Add(aiAnswer);
                        await db.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error generating AI answer: {ex.Message}");
                }
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

            // Previne editarea răspunsurilor AI
            if (IsAiAnswer(answer.UserId))
            {
                TempData["Error"] = "Nu poți edita răspunsurile generate de AI!";
                return RedirectToAction("Show", "Questions", new { id = answer.QuestionId });
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

                // Previne editarea răspunsurilor AI
                if (IsAiAnswer(answer.UserId))
                {
                    TempData["Error"] = "Nu poți edita răspunsurile generate de AI!";
                    return RedirectToAction("Show", "Questions", new { id = answer.QuestionId });
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

            // Previne ștergerea răspunsurilor AI
            if (IsAiAnswer(answer.UserId))
            {
                TempData["Error"] = "Nu poți șterge răspunsurile generate de AI!";
                return RedirectToAction("Show", "Questions", new { id = answer.QuestionId });
            }

            Guid questionId = answer.QuestionId;
            db.Answers.Remove(answer);
            db.SaveChanges();

            return RedirectToAction("Show", "Questions", new { id = questionId });
        }

        // Helper method pentru a verifica dacă un răspuns este de la AI
        public static bool IsAiAnswer(string userId)
        {
            return userId == AI_USER_ID;
        }
    }
}