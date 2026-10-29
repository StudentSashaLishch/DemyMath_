using System.Globalization;
using DemyMath.Infrastructure.Data;
using DemyMath.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using DemyMath.Application.Localization;
using DemyMath.Infrastructure.Localization;
using DemyMath.Application.Topics;
using DemyMath.Infrastructure.Topics;

var builder = WebApplication.CreateBuilder(args);

// Localization
builder.Services.AddLocalization(options =>
{
    options.ResourcesPath = "Resources";
});

builder.Services
    .AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

builder.Services.AddRazorPages();

var supportedCultures = new[]
{
    new CultureInfo("uk"),
    new CultureInfo("en")
};

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("uk");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;

    options.RequestCultureProviders.Insert(
        0,
        new CookieRequestCultureProvider());
});

// Database
var connectionString =
    builder.Configuration.GetConnectionString("DemyMath")
    ?? throw new InvalidOperationException(
        "Connection string 'DemyMath' is not configured.");

builder.Services.AddDbContext<DemyMathDbContext>(options =>
    options.UseNpgsql(connectionString));

// Identity
builder.Services
    .AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;

        options.Password.RequiredLength = 12;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<DemyMathDbContext>();

builder.Services.AddScoped<
    ILocalizedStringService,
    LocalizedStringService>();

builder.Services.AddScoped<
    ITopicQueryService, 
    TopicQueryService>();

builder.Services.AddScoped<DevelopmentDataSeeder>();

var app = builder.Build();

// HTTP pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    await using var scope = app.Services.CreateAsyncScope();

    var seeder = scope.ServiceProvider
        .GetRequiredService<DevelopmentDataSeeder>();

    await seeder.SeedAsync();
}

app.UseRequestLocalization();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();