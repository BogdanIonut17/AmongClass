using Microsoft.AspNetCore.Identity;

namespace AmongClass.Data
{
    // În folderul Migrations, creează DbInitializer.cs
    public static class DbInitializer
    {
        public static async Task SeedAiUser(UserManager<IdentityUser> userManager)
        {
            var aiUser = await userManager.FindByIdAsync("11111111-1111-1111-1111-111111111111");

            if (aiUser == null)
            {
                aiUser = new IdentityUser
                {
                    Id = "11111111-1111-1111-1111-111111111111",
                    UserName = "AI_Assistant",
                    Email = "ai@amongclass.system",
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(aiUser);
            }
        }
    }
}
