using HotelBooking.Web.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// === 1, Razor Pages ===
builder.Services.AddRazorPages(options =>
{
    // All the pages in /Admin required role "Admin"
    options.Conventions.AuthorizeFolder("/Admin", "AdminOnly");
    // Pages booking required any login user
    options.Conventions.AuthorizeFolder("/Booking");
    options.Conventions.AuthorizeFolder("/Rooms/Book");
});

// === 2. Cookie Authentication ===
// Explain how Asp.Net Core handles cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Redirect to autherization
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.Name = "HotelBooking.Auth";
        options.Cookie.HttpOnly = true; //JS disabled used to cookies.
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Only HTTPS
        options.Cookie.SameSite = SameSiteMode.Lax; // Protect from CSRF
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true; // continues during activity
    });

// === 3. Authorization Policy ===
builder.Services.AddAuthorization(options =>
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin")));

// Connection string from config
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// === 4. HTTP Client for email
// MailKit manages the connection itself., httpclient does not need
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // wwwroot/

app.UseRouting();

app.UseAuthentication(); // Who arre You? (reading cookie)
app.UseAuthorization();  // What are You? (checking role/policy)

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
