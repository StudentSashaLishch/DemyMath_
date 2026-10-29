using System.Globalization;
using DemyMath.Application.Topics;
using Microsoft.AspNetCore.Mvc;

namespace DemyMath.Web.Controllers;

public class LearningController : Controller
{
    private readonly ITopicQueryService _topics;

    public LearningController(ITopicQueryService topics)
    {
        _topics = topics;
    }

    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var culture = CultureInfo.CurrentUICulture.Name;

        var topics = await _topics.GetAllAsync(
            culture, cancellationToken);

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

        return View(topic);
    }
}