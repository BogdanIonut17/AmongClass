using Microsoft.AspNetCore.Identity;

namespace AmongClass.Helpers
{
    public static class IdentityDataSeeder
    {
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Lista de roluri de creat
            string[] roleNames = { "Student", "Teacher", "Admin" };

            foreach (var roleName in roleNames)
            {
                // Verifică dacă rolul există
                var roleExist = await roleManager.RoleExistsAsync(roleName);

                if (!roleExist)
                {
                    // Creează rolul dacă nu există
                    var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));

                    if (roleResult.Succeeded)
                    {
                        Console.WriteLine($"Role '{roleName}' created successfully.");
                    }
                    else
                    {
                        Console.WriteLine($"Error creating role '{roleName}'");
                    }
                }
            }
        }

        // Opțional: Metodă pentru a crea un user admin default
        public static async Task SeedAdminUserAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            string adminEmail = "admin@amongclass.com";
            string adminPassword = "Admin123!";

            // Verifică dacă adminul există
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                // Creează adminul
                var newAdmin = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var createAdmin = await userManager.CreateAsync(newAdmin, adminPassword);

                if (createAdmin.Succeeded)
                {
                    // Adaugă adminul la rolul Admin
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                    Console.WriteLine($"Admin user created: {adminEmail}");
                }
            }
        }
    }
}