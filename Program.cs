using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PrintMarket.Data;
using PrintMarket.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
    
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<AppUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();
builder.Services.AddSession(); // Session Servisi Eklendi
builder.Services.AddHttpContextAccessor(); // View'da kullanmak için gerekli

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();

        // Varsayılan Admin Kullanıcısı Kontrolü (Yoksa ekle)
        if (!context.Admins.Any())
        {
            var hasher = new PasswordHasher<object>();
            context.Admins.Add(new Admin { 
                Email = "admin@mail.com", 
                PasswordHash = hasher.HashPassword(new object(), "123") 
            });
            context.SaveChanges();
        }

        // İstenen Kategorilerin Eklenmesi
        string[] categories = { "Tipografi baskı", "Ofset baskı", "Serigrafi baskı" };
        foreach (var catName in categories)
        {
            if (!context.Categories.Any(c => c.Name == catName))
            {
                context.Categories.Add(new Category { Name = catName });
            }
        }
        context.SaveChanges();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

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

app.UseSession(); // Session Middleware Eklendi (Authorization'dan önce olmalı)
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();
