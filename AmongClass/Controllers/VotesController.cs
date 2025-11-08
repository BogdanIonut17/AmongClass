using AmongClass.Data;
using AmongClass.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AmongClass.Controllers
{
    [Authorize]
    public class VotesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public VotesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _db = context;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> Vote(Guid answerId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var answer = await _db.Answers
                .Include(a => a.Question)
                .FirstOrDefaultAsync(a => a.Id == answerId);

            if (answer == null) return NotFound();

            var existingVote = await _db.Votes
                .Include(v => v.Answer)
                .Where(v => v.UserId == currentUser.Id && v.Answer.QuestionId == answer.QuestionId)
                .FirstOrDefaultAsync();

            if (existingVote != null)
            {
                return RedirectToAction("Show", "Questions", new { id = answer.QuestionId });
            }

            if (answer.UserId.ToString() == currentUser.Id)
            {
                return RedirectToAction("Show", "Questions", new { id = answer.QuestionId });
            }

            var vote = new Vote
            {
                Id = Guid.NewGuid(),
                AnswerId = answerId,
                UserId = currentUser.Id,
                VotedAt = DateTime.UtcNow
            };

            _db.Votes.Add(vote);
            await _db.SaveChangesAsync();

            return RedirectToAction("Show", "Questions", new { id = answer.QuestionId });
        }
    }
}