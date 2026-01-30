using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectEmailNight.Context;
using ProjectEmailNight.Entities;
using ProjectEmailNight.Hubs;
using ProjectEmailNight.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAIService, GeminiAIService>();

// DbContext
builder.Services.AddDbContext<EmailContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedEmail = false;
    })
    .AddEntityFrameworkStores<EmailContext>()
    .AddDefaultTokenProviders();

// Cookie ayarları
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
});
builder.Services.AddSignalR();
builder.Services.AddScoped<INotificationService, NotificationService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();  // Bu satırı ekle - UseAuthorization'dan ÖNCE olmalı!
app.UseAuthorization();
app.MapHub<NotificationHub>("/notificationHub");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


// Rolleri ve Admin kullanıcıyı oluştur
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

    // Rolleri oluştur
    string[] roles = { "Admin", "User" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Admin kullanıcı oluştur (yoksa)
    var adminEmail = "admin@emailnight.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    
    if (adminUser == null)
    {
        adminUser = new AppUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            Name = "Admin",
            Surname = "User",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        
        var result = await userManager.CreateAsync(adminUser, "Admin123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}

app.Run();

app.Run();