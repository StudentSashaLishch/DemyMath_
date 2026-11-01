using System.Globalization;
using DemyMath.Application.Localization;
using DemyMath.Application.Tests;
using DemyMath.Application.Topics;
using DemyMath.Infrastructure.Data;
using DemyMath.Infrastructure.Identity;
using DemyMath.Infrastructure.Localization;
using DemyMath.Infrastructure.Tests;
using DemyMath.Infrastructure.Topics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using DemyMath.Infrastructure.Email;
using DemyMath.Application.Identity;

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

builder.Services.Configure<SmtpOptions>(
    builder.Configuration.GetSection("Email:Smtp"));

builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();

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
        options.SignIn.RequireConfirmedAccount = true;
        options.SignIn.RequireConfirmedEmail = true;

        options.Password.RequiredLength = 12;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<DemyMathDbContext>();

// Application services
builder.Services.AddScoped<
    ILocalizedStringService,
    LocalizedStringService>();

builder.Services.AddScoped<
    ITopicQueryService,
    TopicQueryService>();

builder.Services.AddScoped<
    ITestService,
    TestService>();

// Development seeders
builder.Services.AddScoped<DevelopmentDataSeeder>();
builder.Services.AddScoped<DevelopmentTestSeeder>();

builder.Services.AddScoped<IdentitySeeder>();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var identitySeeder = scope.ServiceProvider
        .GetRequiredService<IdentitySeeder>();

    await identitySeeder.SeedAsync();
}

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

    var testSeeder = scope.ServiceProvider
        .GetRequiredService<DevelopmentTestSeeder>();

    await testSeeder.SeedAsync();
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