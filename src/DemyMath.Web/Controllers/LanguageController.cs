
using System;
using System.Linq;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace DemyMath.Web.Controllers;

public class LanguageController : Controller
{
    private static readonly string[] SupportedLanguages =
    {
        "uk",
        "en"
    };

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SetLanguage(
        string culture,
        string? returnUrl = null)
    {
        if (!SupportedLanguages.Contains(culture))
        {
            culture = "uk";
        }

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(
                new RequestCulture(culture)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                HttpOnly = true
            });

        if (!string.IsNullOrWhiteSpace(returnUrl)
            && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }
}
