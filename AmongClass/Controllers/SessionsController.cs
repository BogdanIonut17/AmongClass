using AmongClass.Data;
using AmongClass.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AmongClass.Controllers
{
    public class SessionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser<Guid>> _userManager;

        public SessionController(ApplicationDbContext context, UserManager<IdentityUser<Guid>> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ===========================
        // Profesor: lista sesiunilor create de el
        // ===========================
        public async Task<IActionResult> MySessions()
        {
            var teacherId = Guid.Parse(_userManager.GetUserId(User)!);
            var sessions = await _context.Sessions
                .Where(s => s.TeacherId == teacherId)
                //.Include(s => s.Category)
                .ToListAsync();

            return View(sessions);
        }

        // ===========================
        // Profesor: creează sesiune
        // ===========================
        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Session session)
        {
            if (ModelState.IsValid)
            {
                session.Id = Guid.NewGuid();
                session.TeacherId = Guid.Parse(_userManager.GetUserId(User)!);
                _context.Sessions.Add(session);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(MySessions));
            }
            return View(session);
        }

        // ===========================
        // Student: lista sesiunilor disponibile
        // ===========================
        public async Task<IActionResult> AvailableSessions()
        {
            var studentId = Guid.Parse(_userManager.GetUserId(User)!);

            // exclude sesiunile la care studentul a dat deja join
            var joinedSessionIds = await _context.SessionStudents
                .Where(ss => ss.StudentId == studentId)
                .Select(ss => ss.SessionId)
                .ToListAsync();

            var sessions = await _context.Sessions
                .Where(s => !joinedSessionIds.Contains(s.Id))
                .Include(s => s.Teacher)
                //.Include(s => s.Category)
                .ToListAsync();

            return View(sessions);
        }

        // ===========================
        // Student: da join la sesiune
        // ===========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Join(Guid sessionId)
        {
            var studentId = Guid.Parse(_userManager.GetUserId(User)!);

            // verifică dacă deja a dat join
            var exists = await _context.SessionStudents
                .AnyAsync(ss => ss.SessionId == sessionId && ss.StudentId == studentId);

            if (!exists)
            {
                var join = new SessionStudent
                {
                    SessionId = sessionId,
                    StudentId = studentId
                };
                _context.SessionStudents.Add(join);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(MyJoinedSessions));
        }

        // ===========================
        // Student: lista sesiunilor la care a dat join
        // ===========================
        public async Task<IActionResult> MyJoinedSessions()
        {
            var studentId = Guid.Parse(_userManager.GetUserId(User)!);

            var sessions = await _context.SessionStudents
                .Where(ss => ss.StudentId == studentId)
                .Include(ss => ss.Session)
                    .ThenInclude(s => s.Teacher)
                .Select(ss => ss.Session)
                .ToListAsync();

            return View(sessions);
        }

        // ===========================
        // Detalii sesiune
        // ===========================
        public async Task<IActionResult> Details(Guid id)
        {
            var session = await _context.Sessions
                .Include(s => s.Teacher)
                //.Include(s => s.Category)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (session == null)
                return NotFound();

            return View(session);
        }
    }
}
