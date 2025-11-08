using AmongClass.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AmongClass.Controllers
{
    public class RolesController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RolesController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpPost]
        public async Task<IActionResult> AssignSelf(UserRole role)
        {
            // ✅ 1. Get current user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized("User not logged in");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound("User not found");

            // ✅ 2. Ensure role exists
            var roleName = role.ToString();
            if (!await _roleManager.RoleExistsAsync(roleName))
                return BadRequest($"Role '{roleName}' does not exist");

            // ✅ 3. Assign role
            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // ✅ 4. Redirect after success
            TempData["Message"] = $"Role '{roleName}' assigned successfully!";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AssignSelf()
        {
            return View();
        }
    }
}
