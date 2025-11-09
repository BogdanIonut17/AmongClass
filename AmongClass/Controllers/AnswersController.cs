using AmongClass.Data;
using AmongClass.Helpers;
using AmongClass.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AmongClass.Controllers
{
    public class AnswersController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly RagService _rag;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpClientFactory _httpClientFactory;

        // GUID constant pentru AI - va fi același în toată aplicația
        public static readonly string AI_USER_ID = "11111111-1111-1111-1111-111111111111";

        public AnswersController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            RagService rag,
            IServiceProvider serviceProvider,
            IHttpClientFactory httpClientFactory)
        {
            db = context;
            _userManager = userManager;
            _rag = rag;
            _serviceProvider = serviceProvider;
            _httpClientFactory = httpClientFactory;

            // Asigură-te că user-ul AI există în baza de date
            EnsureAiUserExists().Wait();
        }

        private async Task EnsureAiUserExists()
        {
            try
            {
                var aiUser = await _userManager.FindByIdAsync(AI_USER_ID);

                if (aiUser == null)
                {
                    Console.WriteLine("🤖 Creating AI User...");

                    aiUser = new IdentityUser
                    {
                        Id = AI_USER_ID,
                        UserName = "AI_Assistant",
                        Email = "ai@amongclass.system",
                        EmailConfirmed = true,
                        LockoutEnabled = false,
                    };

                    var result = await _userManager.CreateAsync(aiUser);

                    if (result.Succeeded)
                    {
                        Console.WriteLine("✅ AI User created successfully!");
                    }
                    else
                    {
                        Console.WriteLine($"❌ Failed to create AI User:");
                        foreach (var error in result.Errors)
                        {
                            Console.WriteLine($"   - {error.Description}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("✅ AI User already exists");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error ensuring AI user exists: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
            }
        }

        // Metodă pentru a genera răspuns AI folosind LLM (ca în OllamaController)
        private async Task<string> GenerateAiResponseWithLLM(string questionText, RagService ragService, HttpClient httpClient)
        {
            try
            {
                Console.WriteLine($"🧠 Getting relevant rules for: {questionText}");

                // Obține regulile relevante din RAG
                string rules = await ragService.GetRelevantRules(questionText);

                Console.WriteLine($"📚 Rules retrieved (length: {rules.Length})");

                // Construiește prompt-ul complet
                string fullPrompt = $"Rules:\n{rules}\n\nQuestion: {questionText}";

                Console.WriteLine($"📝 Full prompt created (length: {fullPrompt.Length})");

                // Creează request-ul pentru Ollama
                var request = new
                {
                    model = "qwen2.5",
                    prompt = fullPrompt,
                    stream = false
                };

                Console.WriteLine($"🚀 Sending request to Ollama...");

                // Apelează Ollama
                var response = await httpClient.PostAsJsonAsync("http://localhost:11434/api/generate", request);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ Ollama Error: {error}");
                    return $"Error generating AI response: {error}";
                }

                // Parse răspunsul
                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                string modelResponse = doc.RootElement.GetProperty("response").GetString();

                Console.WriteLine($"✅ LLM response received (length: {modelResponse?.Length ?? 0})");

                return modelResponse ?? "No response from AI";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GenerateAiResponseWithLLM: {ex.Message}");
                Console.WriteLine($"   Stack: {ex.StackTrace}");
                return $"Error generating AI response: {ex.Message}";
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
            Console.WriteLine($"🔵 New answer POST started for question {answer.QuestionId}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine($"❌ ModelState is invalid:");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"   - {error.ErrorMessage}");
                }
                return View(answer);
            }

            try
            {
                answer.Id = Guid.NewGuid();
                answer.UserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;

                Console.WriteLine($"🔵 Answer ID: {answer.Id}");
                Console.WriteLine($"🔵 User ID: {answer.UserId}");
                Console.WriteLine($"🔵 Question ID: {answer.QuestionId}");
                Console.WriteLine($"🔵 Answer Text: {(answer.Text != null ? answer.Text.Substring(0, Math.Min(50, answer.Text.Length)) : "null")}...");

                db.Answers.Add(answer);
                await db.SaveChangesAsync();

                Console.WriteLine($"✅ User answer saved successfully for question {answer.QuestionId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error saving user answer:");
                Console.WriteLine($"   Message: {ex.Message}");
                Console.WriteLine($"   Stack: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"   Inner: {ex.InnerException.Message}");
                }
                throw;
            }

            // Generează răspuns AI în fundal - CU SCOPE NOU și LLM CALL!
            var questionId = answer.QuestionId;
            _ = Task.Run(async () =>
            {
                try
                {
                    Console.WriteLine($"🤖 Starting AI answer generation for question {questionId}...");

                    // Așteaptă puțin ca răspunsul user-ului să fie complet salvat
                    await Task.Delay(500);

                    // IMPORTANT: Creează un scope NOU pentru background task
                    using var scope = _serviceProvider.CreateScope();
                    var scopedDb = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var scopedRag = scope.ServiceProvider.GetRequiredService<RagService>();
                    var scopedHttpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
                    var httpClient = scopedHttpClientFactory.CreateClient();

                    // Verifică dacă AI-ul a răspuns deja
                    var existingAiAnswer = await scopedDb.Answers
                        .FirstOrDefaultAsync(a => a.QuestionId == questionId && a.UserId == AI_USER_ID);

                    if (existingAiAnswer != null)
                    {
                        Console.WriteLine($"⚠️ AI already answered question {questionId}, skipping");
                        return;
                    }

                    // Obține întrebarea
                    var question = await scopedDb.Questions.FindAsync(questionId);
                    if (question == null)
                    {
                        Console.WriteLine($"❌ Question {questionId} not found");
                        return;
                    }

                    Console.WriteLine($"📝 Question text: {question.Text}");

                    // IMPORTANT: Generează răspunsul AI folosind LLM (ca în OllamaController)
                    string aiResponseText = await GenerateAiResponseWithLLM(question.Text, scopedRag, httpClient);

                    Console.WriteLine($"✅ AI response generated: {aiResponseText.Substring(0, Math.Min(100, aiResponseText.Length))}...");

                    // Creează răspunsul AI
                    var aiAnswer = new Answer
                    {
                        Id = Guid.NewGuid(),
                        Text = aiResponseText,
                        QuestionId = questionId,
                        UserId = AI_USER_ID
                    };

                    scopedDb.Answers.Add(aiAnswer);
                    await scopedDb.SaveChangesAsync();

                    Console.WriteLine($"✅ AI answer saved successfully for question {questionId}!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error generating AI answer:");
                    Console.WriteLine($"   Message: {ex.Message}");
                    Console.WriteLine($"   Stack: {ex.StackTrace}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"   Inner: {ex.InnerException.Message}");
                    }
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