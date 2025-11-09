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

        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Index()
        {
            var teacherId = _userManager.GetUserId(User)!;
            var sessions = await _context.Sessions
                .Where(s => s.TeacherId == teacherId)
                .Include(s => s.SessionQuestions)
                    .ThenInclude(sq => sq.Question)
                .Include(s => s.SessionStudents)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return View(sessions);
        }

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
                session.TeacherId = _userManager.GetUserId(User)!;
                session.JoinCode = GenerateJoinCode();
                session.Status = SessionStatus.Inactive;
                session.CreatedAt = DateTime.UtcNow;

                _context.Sessions.Add(session);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Setup), new { id = session.Id });
            }
            return View(session);
        }

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

            var allQuestions = await _context.Questions.ToListAsync();
            ViewBag.AvailableQuestions = allQuestions;
            ViewBag.Session = session;

            return View(session);
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddQuestion(Guid sessionId, Guid questionId, int timeLimit)
        {
            var session = await _context.Sessions
                .Include(s => s.SessionQuestions)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null)
                return NotFound();

            var exists = session.SessionQuestions.Any(sq => sq.QuestionId == questionId);
            if (exists)
                return RedirectToAction(nameof(Setup), new { id = sessionId });

            var maxOrder = session.SessionQuestions.Any()
                ? session.SessionQuestions.Max(sq => sq.Order)
                : 0;

            var sessionQuestion = new SessionQuestion
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                QuestionId = questionId,
                Order = maxOrder + 1,
                TimeLimit = timeLimit
            };

            _context.SessionQuestions.Add(sessionQuestion);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Setup), new { id = sessionId });
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveQuestion(Guid id)
        {
            var sessionQuestion = await _context.SessionQuestions.FindAsync(id);

            if (sessionQuestion == null)
                return NotFound();

            var sessionId = sessionQuestion.SessionId;
            _context.SessionQuestions.Remove(sessionQuestion);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Setup), new { id = sessionId });
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartSession(Guid id)
        {
            var session = await _context.Sessions
                .Include(s => s.SessionQuestions)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (session == null)
                return NotFound();

            if (session.Status == SessionStatus.Inactive)
            {
                session.Status = SessionStatus.Active;
                var firstQuestion = session.SessionQuestions
                    .OrderBy(sq => sq.Order)
                    .FirstOrDefault();

                if (firstQuestion != null)
                {
                    session.CurrentQuestionId = firstQuestion.QuestionId;
                    firstQuestion.ActivatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Dashboard), new { id = session.Id });
        }

        [HttpGet]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Dashboard(Guid id)
        {
            var session = await _context.Sessions
                .Include(s => s.Scores)
                    .ThenInclude(sc => sc.Student)
                .Include(s => s.SessionQuestions)
                    .ThenInclude(sq => sq.Question)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (session == null || session.Status != SessionStatus.Active)
            {
                return RedirectToAction(nameof(Index));
            }

            var currentQuestionId = session.CurrentQuestionId;

            if (currentQuestionId.HasValue)
            {
                var currentQuestion = await _context.Questions
                    .Where(q => q.Id == currentQuestionId.Value)
                    .Include(q => q.Answers)
                        .ThenInclude(a => a.User)
                    .Include(q => q.Answers)
                        .ThenInclude(a => a.Votes)
                    .FirstOrDefaultAsync();

                ViewBag.CurrentQuestion = currentQuestion;
            }
            else
            {
                ViewBag.CurrentQuestion = null;
            }

            ViewBag.Scores = session.Scores
                .OrderByDescending(s => s.Points)
                .ToList();

            return View(session);
        }

        // În LiveSessionController.cs
        [HttpPost]
        [Authorize(Roles = "Teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NextQuestion(Guid sessionId)
        {
            var session = await _context.Sessions
                .Include(s => s.SessionQuestions)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null)
                return NotFound();

            var currentSessionQuestion = session.SessionQuestions
                .FirstOrDefault(sq => sq.QuestionId == session.CurrentQuestionId);

            if (currentSessionQuestion != null)
            {
                currentSessionQuestion.CompletedAt = DateTime.UtcNow;
            }

            var nextQuestion = session.SessionQuestions
                .Where(sq => sq.Order > (currentSessionQuestion?.Order ?? 0))
                .OrderBy(sq => sq.Order)
                .FirstOrDefault();

            if (nextQuestion != null)
            {
                session.CurrentQuestionId = nextQuestion.QuestionId;
                nextQuestion.ActivatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Dashboard), new { id = sessionId });
            }
            else
            {
                return RedirectToAction(nameof(EndSession), new { id = sessionId });
            }
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
                session.CurrentQuestionId = null;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Results), new { id = id });
        }

        // LiveSessionController.cs

        // ... după metoda EndSession (sau unde dorești) ...

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var session = await _context.Sessions.FindAsync(id);

            if (session == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var teacherId = _userManager.GetUserId(User)!;
            if (session.TeacherId != teacherId)
            {
                return Forbid(); 
            }

            if (session.Status == SessionStatus.Active)
            {
                TempData["ErrorMessage"] = $"Nu poți șterge sesiunea '{session.Name}' deoarece este activă. Trebuie să o închei mai întâi.";
                return RedirectToAction(nameof(Index));
            }

            _context.Sessions.Remove(session);

            _context.SaveChanges();

            TempData["SuccessMessage"] = $"Sesiunea '{session.Name}' a fost ștearsă cu succes.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Results(Guid id)
        {
            var session = await _context.Sessions
                .Include(s => s.SessionStudents)
                    .ThenInclude(ss => ss.Student)
                .Include(s => s.SessionQuestions)
                    .ThenInclude(sq => sq.Question)
                        .ThenInclude(q => q.Answers)
                            .ThenInclude(a => a.Votes)
                .Include(s => s.SessionQuestions)
                    .ThenInclude(sq => sq.Question)
                        .ThenInclude(q => q.Answers)
                            .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (session == null)
                return NotFound();

            return View(session);
        }

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

            var studentId = _userManager.GetUserId(User)!;

            var exists = await _context.SessionStudents
                .AnyAsync(ss => ss.SessionId == session.Id && ss.StudentId == studentId);

            if (!exists)
            {
                var sessionStudent = new SessionStudent
                {
                    SessionId = session.Id,
                    StudentId = studentId
                };
                _context.SessionStudents.Add(sessionStudent);

                var score = new Score
                {
                    Id = Guid.NewGuid(),
                    SessionId = session.Id,
                    StudentId = studentId,
                    Points = 0
                };
                _context.Scores.Add(score);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Play), new { id = session.Id });
        }

        [HttpGet]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Play(Guid id)
        {
            var studentId = _userManager.GetUserId(User)!;

            var session = await _context.Sessions
                .Include(s => s.SessionQuestions)
                    .ThenInclude(sq => sq.Question)
                        .ThenInclude(q => q.Answers)
                            .ThenInclude(a => a.Votes)
                .Include(s => s.SessionStudents)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (session == null)
                return NotFound();

            var isInSession = session.SessionStudents.Any(ss => ss.StudentId == studentId);

            if (!isInSession)
                return RedirectToAction(nameof(Join));

            var studentScore = await _context.Scores
                .FirstOrDefaultAsync(sc => sc.SessionId == session.Id && sc.StudentId == studentId);

            ViewBag.StudentId = studentId;
            ViewBag.StudentScore = studentScore;

            return View(session);
        }

        [HttpPost]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> SubmitAnswer(Guid sessionId, Guid questionId, string answerText)
        {
            var studentId = _userManager.GetUserId(User)!;

            var existingAnswer = await _context.Answers
                .FirstOrDefaultAsync(a => a.QuestionId == questionId && a.UserId == studentId);

            if (existingAnswer != null)
                return RedirectToAction(nameof(Play), new { id = sessionId });

            var answer = new Answer
            {
                Id = Guid.NewGuid(),
                Text = answerText,
                QuestionId = questionId,
                UserId = studentId
            };

            _context.Answers.Add(answer);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Play), new { id = sessionId });
        }

        [HttpPost]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> VoteAnswer(Guid sessionId, Guid answerId)
        {
            var studentId = _userManager.GetUserId(User)!;

            var answer = await _context.Answers
                .Include(a => a.Question)
                .FirstOrDefaultAsync(a => a.Id == answerId);

            if (answer == null)
                return NotFound();

            var existingVote = await _context.Votes
                .Include(v => v.Answer)
                .FirstOrDefaultAsync(v => v.UserId == studentId && v.Answer.QuestionId == answer.QuestionId);

            if (existingVote != null)
                return RedirectToAction(nameof(Play), new { id = sessionId });

            if (answer.UserId == studentId)
                return RedirectToAction(nameof(Play), new { id = sessionId });

            var vote = new Vote
            {
                Id = Guid.NewGuid(),
                AnswerId = answerId,
                UserId = studentId,
                VotedAt = DateTime.UtcNow
            };

            _context.Votes.Add(vote);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Play), new { id = sessionId });
        }

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