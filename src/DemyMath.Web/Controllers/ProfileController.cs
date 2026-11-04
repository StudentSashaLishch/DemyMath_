using DemyMath.Application.Progress;
using DemyMath.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DemyMath.Web.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly IUserProgressService _progress;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public ProfileController(
        IUserProgressService progress,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _progress = progress;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var userId = _userManager.GetUserId(User)!;

        var summary = await _progress.GetSummaryAsync(
            userId,
            cancellationToken);

        return View(summary);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetProgress(
        CancellationToken cancellationToken)
    {
        var userId = _userManager.GetUserId(User)!;

        await _progress.ResetAsync(userId, cancellationToken);

        TempData["ProfileMessage"] = "ProgressReset";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAccount()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return NotFound();
        }

        var result = await _userManager.DeleteAsync(user);

        if (!result.Succeeded)
        {
            TempData["ProfileMessage"] = "AccountDeleteFailed";
            return RedirectToAction(nameof(Index));
        }

        await _signInManager.SignOutAsync();

        return RedirectToAction("Index", "Home");
    }
}