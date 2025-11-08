using AmongClass.Data;
using AmongClass.Helpers;
using AmongClass.IRepository;
using AmongClass.Repository;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// MODIFICAT: Schimbă RequireConfirmedAccount la false pentru a nu cere confirmare email
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;  // SCHIMBAT de la true la false
    // Configurări opționale pentru parole mai simple în development
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

//custom
builder.Services.AddSingleton<RagService>();
builder.Services.AddMapster();
builder.Services.AddHttpClient();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

var app = builder.Build();

//custom - Seed roles
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    try
    {
        // Ensure database is migrated (optional, but useful for initial setup)
        var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate();

        // Seed roles
        await IdentityDataSeeder.SeedRolesAsync(serviceProvider);
    }
    catch (Exception ex)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database with roles.");
    }
}

//var rag = app.Services.GetRequiredService<RagService>();
//await rag.InitAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();