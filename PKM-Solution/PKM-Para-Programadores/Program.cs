using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PKM_Project.Data;
using PKM_Project.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// DB Context
builder.Services.AddDbContext<PKMDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PKMDbContext")));

// Identity
builder.Services.AddIdentityCore<Usuario>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 3;
        options.Password.RequireUppercase = false;
    }
    )
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<PKMDbContext>();
//.AddSignInManager();

// Cookies
builder.Services.AddAuthentication(opt =>
{
    opt.DefaultScheme = IdentityConstants.ApplicationScheme;
})
.AddIdentityCookies();


builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
    options.LoginPath = "/Usuario/Login";
    options.AccessDeniedPath = "/Usuario/AccessDenied";
});



var app = builder.Build();

// Db Seeder
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<PKMDbContext>();
        var userManager = services.GetRequiredService<UserManager<Usuario>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await DbSeeder.Seed(context, userManager, roleManager);
    }
    catch
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError("An error occurred seeding the DB.");
    }
}




// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
