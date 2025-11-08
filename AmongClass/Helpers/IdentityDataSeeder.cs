using Microsoft.AspNetCore.Identity;

namespace AmongClass.Helpers
{
    public static class IdentityDataSeeder
    {
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));

            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            //var logger = serviceProvider.GetService<ILogger<IdentityDataSeeder>>();

            string[] roles = new[] { "Admin", "Student", "Teacher" };

            foreach (var roleName in roles)
            {
                try
                {
                    var exists = await roleManager.RoleExistsAsync(roleName);
                    if (!exists)
                    {
                        var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                        if (!result.Succeeded)
                        {
                            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                            //logger?.LogError("Failed to create role '{Role}': {Errors}", roleName, errors);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //logger?.LogError(ex, "Exception while ensuring role '{Role}' exists.", roleName);
                    // Rethrow so caller (Program.cs) can handle/log if desired
                    throw;
                }
            }
        }
    }
}
