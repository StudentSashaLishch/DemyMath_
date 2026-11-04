using System.Globalization;
using DemyMath.Application.Progress;
using DemyMath.Application.Tests;
using DemyMath.Application.Tests.Models;
using DemyMath.Application.Topics;
using DemyMath.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DemyMath.Web.Controllers;

public class LearningController : Controller
{
    private readonly ITopicQueryService _topics;
    private readonly ITestService _tests;
    private readonly IUserProgressService _progress;

    public LearningController(
        ITopicQueryService topics,
        ITestService tests,
        IUserProgressService progress)
    {
        _topics = topics;
        _tests = tests;
        _progress = progress;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var culture = CultureInfo.CurrentUICulture.Name;
        var topics = await _topics.GetAllAsync(culture, cancellationToken);

        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var completedKeys = await _progress.GetCompletedProgressKeysAsync(
                userId,
                cancellationToken);

            foreach (var topic in topics)
            {
                topic.IsCompleted = completedKeys.Contains(topic.ProgressKey);
            }
        }

        return View(topics);
    }

    public async Task<IActionResult> Topic(
        int id,
        CancellationToken cancellationToken)
    {
        var culture = CultureInfo.CurrentUICulture.Name;

        var topic = await _topics.GetByIdAsync(
            id, culture, cancellationToken);

        if (topic is null)
        {
            return NotFound();
        }

        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            await _progress.MarkCompletedIfNoTestAsync(
                userId,
                id,
                cancellationToken);
        }

        return View(topic);
    }

    [HttpGet]
    public async Task<IActionResult> Test(
        int id,
        CancellationToken cancellationToken)
    {
        var culture = CultureInfo.CurrentUICulture.Name;

        var test = await _tests.GetByTopicIdAsync(
            id, culture, cancellationToken);

        if (test is null)
        {
            return NotFound();
        }

        return View(new TestPageViewModel
        {
            Test = test
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Test(
        int id,
        TestSubmissionDto submission,
        CancellationToken cancellationToken)
    {
        var culture = CultureInfo.CurrentUICulture.Name;

        var result = await _tests.SubmitAsync(
            id, submission, culture, cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        var test = await _tests.GetByTopicIdAsync(
            id, culture, cancellationToken);

        if (test is null)
        {
            return NotFound();
        }

        if (User.Identity?.IsAuthenticated == true &&
            result.TotalQuestions > 0 &&
            result.CorrectAnswers == result.TotalQuestions)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            await _progress.MarkCompletedAsync(
                userId,
                id,
                cancellationToken);
        }

        return View(new TestPageViewModel
        {
            Test = test,
            Result = result,
            Submission = submission
        });
    }
}