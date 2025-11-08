using AmongClass.Data;
using AmongClass.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AmongClass.Controllers
{
    public class ScoreController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ScoreController(ApplicationDbContext context)
        {
            _context = context;
        }

        // /Score/Index?sessionId=...
        public async Task<IActionResult> Index(Guid sessionId)
        {
            var leaderboard = await _context.Scores
                .Where(s => s.SessionId == sessionId)
                .Include(s => s.Student)
                .OrderByDescending(s => s.Points)
                .Select(s => new ScoreViewModel
                {
                    StudentName = s.Student.UserName,
                    Points = s.Points
                })
                .ToListAsync();

            return View(leaderboard);
        }
    }
}
