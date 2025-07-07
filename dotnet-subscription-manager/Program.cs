using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using dotnet_subscription_manager.Data;
using dotnet_subscription_manager.Models;

var builder = WebApplication.CreateBuilder(args);

// add mvc
builder.Services.AddControllersWithViews();

// setup sqlite database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=subscriptions.db"));

// setup identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // make password requirements simple for testing
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 4;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// setup login path
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
});

var app = builder.Build();

// create database if doesn't exist
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
