using Microsoft.EntityFrameworkCore;
using Pharm.Data; // Ensure this matches your namespace

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Register PharmContext with the connection string
builder.Services.AddDbContext<PharmContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication("Cookies")
    .AddCookie(options =>
    {
        options.LoginPath = "/Accounts/Login";
        options.LogoutPath = "/Accounts/Logout";
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
