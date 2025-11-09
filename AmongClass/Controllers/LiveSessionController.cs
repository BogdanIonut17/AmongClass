using AmongClass.Data;
using AmongClass.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System;

namespace AmongClass.Controllers
{
    [Authorize]
    public class LiveSessionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public LiveSessionController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ===========================
        // Profesor: Listă sesiuni live
        // ===========================
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Index()
        {
            // ❌ EROARE CORECTATĂ: Elimină Guid.Parse
            var teacherId = _userManager.GetUserId(User)!;

            // Folosim teacherId (string). Presupunem că Session.TeacherId e string.
            var sessions = await _context.Sessions
                .Where(s => s.TeacherId == teacherId)
                .Include(s => s.SessionQuestions)
                    .ThenInclude(sq => sq.Question)
                .Include(s => s.SessionStudents)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return View(sessions);
        }

        // ===========================
        // Profesor: Creează sesiune live
        // ===========================
        [HttpGet]
        [Authorize(Roles = "Teacher")]
        public IActionResult Create() => View();

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Session session)
        {
            if (ModelState.IsValid || true)
            {
                session.Id = Guid.NewGuid();
                // ❌ EROARE CORECTATĂ: TeacherId este string
                session.TeacherId = _userManager.GetUserId(User)!;
                session.JoinCode = GenerateJoinCode();
                session.Status = SessionStatus.Inactive;
                session.CreatedAt = DateTime.UtcNow;

                _context.Sessions.Add(session);
                await _context.SaveChangesAsync();

                // ✅ EROARE CS0103 CORECTATĂ: Redirecționează către noua acțiune Setup de mai jos.
                return RedirectToAction(nameof(Setup), new { id = session.Id });
            }
            return View(session);
        }

        // ===========================
        // Profesor: Setup întrebări pentru sesiune (LIPSEA!)
        // ===========================
        [HttpGet]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Setup(Guid id)
        {
            var session = await _context.Sessions
                .Include(s => s.SessionQuestions)
                .ThenInclude(sq => sq.Question)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (session == null)
                return NotFound();

            // Obține toate întrebările disponibile
            var allQuestions = await _context.Questions.ToListAsync();
            ViewBag.AvailableQuestions = allQuestions;
            ViewBag.Session = session;

            return View(session);
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartSession(Guid id)
        {
            var session = await _context.Sessions
                .Include(s => s.SessionQuestions) // Include întrebările pentru a seta CurrentQuestionId
                .FirstOrDefaultAsync(s => s.Id == id);

            if (session == null)
                return NotFound();

            if (session.Status == SessionStatus.Inactive)
            {
                session.Status = SessionStatus.Active;
                // Setează prima întrebare ca fiind activă, dacă există
                session.CurrentQuestionId = session.SessionQuestions
                    .OrderBy(sq => sq.Order)
                    .FirstOrDefault()?.QuestionId;

                await _context.SaveChangesAsync();
            }

            // Redirecționează către noua pagină de monitorizare/administrare
            return RedirectToAction(nameof(Dashboard), new { id = session.Id });
        }

        // ===========================
        // Profesor: Dashboard Sesiune Live
        // ===========================
        [HttpGet]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Dashboard(Guid id)
        {
            // LiveSessionController.cs

            var session = await _context.Sessions
                // 1. Incarcă scorurile (nu este necesară filtrarea aici)
                .Include(s => s.Scores)
                    .ThenInclude(sc => sc.Student)

                // 2. Incarcă toate SessionQuestions (nu aplica filtrare complexă aici)
                .Include(s => s.SessionQuestions)
                    .ThenInclude(sq => sq.Question)

                // 3. Filtrează entitatea principală
                .FirstOrDefaultAsync(s => s.Id == id);

            if (session == null || session.Status != SessionStatus.Active)
            {
                // Dacă sesiunea nu există sau nu este activă, redirecționează către lista de sesiuni
                return RedirectToAction(nameof(Index));
            }

            // Continuarea codului din LiveSessionController.cs Dashboard

            // Găsește ID-ul întrebării curente
            var currentQuestionId = session.CurrentQuestionId;

            if (currentQuestionId.HasValue)
            {
                // Încarcă Întrebarea Curentă împreună cu Răspunsurile și Utilizatorii Răspunsurilor
                var currentQuestion = await _context.Questions
                    .Where(q => q.Id == currentQuestionId.Value)
                    .Include(q => q.Answers)
                        .ThenInclude(a => a.User) // Încarcă utilizatorul care a dat răspunsul
                    .Include(q => q.Answers)
                        .ThenInclude(a => a.Votes) // Încarcă voturile pentru răspuns
                    .FirstOrDefaultAsync();

                ViewBag.CurrentQuestion = currentQuestion;
            }
            else
            {
                ViewBag.CurrentQuestion = null;
            }

            // Sortează scorurile studenților
            ViewBag.Scores = session.Scores
                .OrderByDescending(s => s.Points)
                .ToList();

            return View(session);
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EndSession(Guid id)
        {
            var session = await _context.Sessions.FindAsync(id);

            if (session == null)
                return NotFound();

            if (session.Status == SessionStatus.Active)
            {
                session.Status = SessionStatus.Completed;
                // Opțional: resetează CurrentQuestionId
                session.CurrentQuestionId = null;
                await _context.SaveChangesAsync();
            }

            // Redirecționează către pagina de rezultate (Score/Index) sau înapoi la lista de sesiuni
            return RedirectToAction(nameof(Index));
        }

        // ... (Alte metode pentru profesor: AddQuestion, RemoveQuestion, etc. sunt lăsate neschimbate)
        // Metoda Join este cea cu majoritatea erorilor de tipologie.

        // ===========================
        // Student: Join cu cod
        // ===========================
        [HttpGet]
        [Authorize(Roles = "Student")]
        public IActionResult Join()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Student")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Join(string joinCode)
        {
            if (string.IsNullOrWhiteSpace(joinCode))
            {
                ViewBag.Error = "Te rog introdu un cod valid!";
                return View();
            }

            var session = await _context.Sessions
                .FirstOrDefaultAsync(s => s.JoinCode == joinCode.ToUpper() && s.Status == SessionStatus.Active);

            if (session == null)
            {
                ViewBag.Error = "Codul nu este valid sau sesiunea nu este activă!";
                return View();
            }

            // ❌ EROARE CORECTATĂ: studentId este string (CS0029)
            var studentId = _userManager.GetUserId(User)!;

            // Verifică dacă studentul a dat deja join
            // ❌ EROARE CORECTATĂ: Comparație string-string (CS0019)
            var exists = await _context.SessionStudents
                .AnyAsync(ss => ss.SessionId == session.Id && ss.StudentId == studentId);

            if (!exists)
            {
                var sessionStudent = new SessionStudent
                {
                    SessionId = session.Id,
                    StudentId = studentId // String
                };
                _context.SessionStudents.Add(sessionStudent);

                // Creează scor inițial pentru student
                var score = new Score
                {
                    Id = Guid.NewGuid(),
                    SessionId = session.Id,
                    StudentId = studentId, // String
                    Points = 0
                };
                _context.Scores.Add(score);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Play), new { id = session.Id });
        }

        // ===========================
        // Student: Play sesiune live
        // ===========================
        [HttpGet]
        [Authorize(Roles = "Student")]
        // LiveSessionController.cs

        [HttpGet]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Play(Guid id)
        {
            // Obține ID-ul studentului (string)
            var studentId = _userManager.GetUserId(User)!;

            // 1. Interogarea Sesiunii: Include toate relațiile necesare
            var session = await _context.Sessions
                .Include(s => s.SessionQuestions)
                    .ThenInclude(sq => sq.Question)
                        .ThenInclude(q => q.Answers) // Include Answers
                            .ThenInclude(a => a.Votes) // Include Votes (dacă sunt necesare în View)
                .Include(s => s.SessionStudents) // Pentru verificarea isInSession
                                                 // ❌ ELIMINĂ Include(s => s.Scores): Le vom încărca separat
                .FirstOrDefaultAsync(s => s.Id == id);

            if (session == null)
                return NotFound();

            // 2. Verifică dacă studentul face parte din sesiune
            // Folosim colecția încărcată de mai sus sau interogarea separată (cea încărcată e mai eficientă)
            var isInSession = session.SessionStudents.Any(ss => ss.StudentId == studentId);

            if (!isInSession)
                return RedirectToAction(nameof(Join));

            // 3. Încarcă SCORUL Studentului separat (Cea mai sigură cale)
            var studentScore = await _context.Scores
                .FirstOrDefaultAsync(sc => sc.SessionId == session.Id && sc.StudentId == studentId);

            // 4. Trimite datele către View
            ViewBag.StudentId = studentId;
            ViewBag.StudentScore = studentScore; // Trimite obiectul Score direct (poate fi null)

            // ATENȚIE: Nu mai trimitem `session` (care NU mai include Scores)
            // Va trebui să adaptezi View-ul pentru a folosi ViewBag.StudentScore
            return View(session);
        }

        // ===========================
        // Student: Submit răspuns
        // ===========================
        [HttpPost]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> SubmitAnswer(Guid sessionId, Guid questionId, string answerText)
        {
            // ❌ EROARE CORECTATĂ: studentId este string
            var studentId = _userManager.GetUserId(User)!;

            // Verifică dacă studentul a răspuns deja
            // ❌ EROARE CORECTATĂ: Comparație string-string (CS0019)
            var existingAnswer = await _context.Answers
                .FirstOrDefaultAsync(a => a.QuestionId == questionId && a.UserId == studentId);

            if (existingAnswer != null)
                return RedirectToAction(nameof(Play), new { id = sessionId });

            var answer = new Answer
            {
                Id = Guid.NewGuid(),
                Text = answerText,
                QuestionId = questionId,
                UserId = studentId // String
            };

            _context.Answers.Add(answer);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Play), new { id = sessionId });
        }

        // ===========================
        // Student: Vote răspuns
        // ===========================
        [HttpPost]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> VoteAnswer(Guid sessionId, Guid answerId)
        {
            // ❌ EROARE CORECTATĂ: studentId este string
            var studentId = _userManager.GetUserId(User)!;

            var answer = await _context.Answers
                .Include(a => a.Question)
                .FirstOrDefaultAsync(a => a.Id == answerId);

            if (answer == null)
                return NotFound();

            // Verifică dacă a votat deja pentru această întrebare
            var existingVote = await _context.Votes
                .Include(v => v.Answer)
                // UserId este string, studentId este string. Comparație directă.
                .FirstOrDefaultAsync(v => v.UserId == studentId && v.Answer.QuestionId == answer.QuestionId);

            if (existingVote != null)
                return RedirectToAction(nameof(Play), new { id = sessionId });

            // Nu poți vota propriul răspuns
            if (answer.UserId == studentId)
                return RedirectToAction(nameof(Play), new { id = sessionId });

            var vote = new Vote
            {
                Id = Guid.NewGuid(),
                AnswerId = answerId,
                UserId = studentId, // String
                VotedAt = DateTime.UtcNow
            };

            _context.Votes.Add(vote);

            // TODO: Actualizează scorul în funcție de dacă a ghicit AI-ul sau nu

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Play), new { id = sessionId });
        }

        // ===========================
        // Helper: Generare cod de join
        // ===========================
        private string GenerateJoinCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var code = new StringBuilder(6);

            for (int i = 0; i < 6; i++)
            {
                code.Append(chars[random.Next(chars.Length)]);
            }

            return code.ToString();
        }

        // ===========================
        // API: Get current question (pentru polling)
        // ===========================
        [HttpGet]
        public async Task<IActionResult> GetCurrentQuestion(Guid sessionId)
        {
            var session = await _context.Sessions
                .Include(s => s.SessionQuestions)
                    .ThenInclude(sq => sq.Question)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null || !session.CurrentQuestionId.HasValue)
                return Json(new { hasQuestion = false });

            var currentQuestion = session.SessionQuestions
                .FirstOrDefault(sq => sq.QuestionId == session.CurrentQuestionId.Value);

            return Json(new
            {
                hasQuestion = true,
                questionId = currentQuestion.QuestionId,
                questionText = currentQuestion.Question.Text,
                timeLimit = currentQuestion.TimeLimit,
                activatedAt = currentQuestion.ActivatedAt
            });
        }
    }
}