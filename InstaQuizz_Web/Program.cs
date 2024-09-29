using InstaQuiz.Data;
using InstaQuiz.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure EF Core with SQL Server
builder.Services.AddDbContext<InstaQuizContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("InstaQuizConnection")));

// Register MLService with HttpClient
builder.Services.AddHttpClient<MLService>();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
    options.Cookie.HttpOnly = true; // Cookie only accessible to the server
    options.Cookie.IsEssential = true; // Ensure session is used even if tracking is disabled
});

var app = builder.Build();

// Ensure the database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<InstaQuizContext>();
    dbContext.Database.EnsureCreated();  // This will create the database and tables if they don't exist
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Login}/{id?}");

app.Run();
