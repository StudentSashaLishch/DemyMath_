
using System.Globalization;
using DemyMath.Application.ContentVersions;
using DemyMath.Application.Topics;
using DemyMath.Application.Topics.Models;
using DemyMath.Application.Tests;
using DemyMath.Application.Tests.Models;
using DemyMath.Domain.Entities;
using DemyMath.Models.Admin;
using DemyMath.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DemyMath.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ILearningContentVersionService _versionService;
    private readonly ITopicQueryService _topicQueryService;
    private readonly ITopicManagementService _topicManagementService;
    private readonly ITestManagementService _testManagementService;
    private readonly RequestLocalizationOptions _localizationOptions;

    public AdminController(
        ILearningContentVersionService versionService,
        ITopicQueryService topicQueryService,
        ITopicManagementService topicManagementService,
        ITestManagementService testManagementService,
        IOptions<RequestLocalizationOptions> localizationOptions)
    {
        _versionService = versionService;
        _topicQueryService = topicQueryService;
        _topicManagementService = topicManagementService;
        _testManagementService = testManagementService;
        _localizationOptions = localizationOptions.Value;
    }

    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        await _versionService.EnsureDraftAsync(cancellationToken);

        var topics = await _topicQueryService.GetAllAsync(
            CultureInfo.CurrentUICulture.Name,
            cancellationToken,
            ContentVersionStatus.Draft);

        return View(topics);
    }

    [HttpGet]
    public async Task<IActionResult> CreateTopic(
        CancellationToken cancellationToken)
    {
        await _versionService.EnsureDraftAsync(cancellationToken);

        var cultures = GetSupportedCultures();

        var topic = await _topicManagementService.GetForCreateAsync(cultures);

        return View("EditTopic", await BuildViewModelAsync(
            topic,
            cultures,
            cancellationToken));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTopic(
        AdminTopicEditViewModel model,
        CancellationToken cancellationToken)
    {
        await _versionService.EnsureDraftAsync(cancellationToken);

        return await SaveTopicAsync(model, cancellationToken);
    }

    [HttpGet]
    public async Task<IActionResult> EditTopic(
        int id,
        CancellationToken cancellationToken)
    {
        await _versionService.EnsureDraftAsync(cancellationToken);

        var cultures = GetSupportedCultures();

        var topic = await _topicManagementService.GetForEditAsync(
            id,
            CultureInfo.CurrentUICulture.Name,
            cultures,
            cancellationToken);

        if (topic is null)
            return NotFound();

        return View(await BuildViewModelAsync(
            topic,
            cultures,
            cancellationToken));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTopic(
        int id,
        AdminTopicEditViewModel model,
        CancellationToken cancellationToken)
    {
        await _versionService.EnsureDraftAsync(cancellationToken);

        if (model.Topic.Id != id)
            return BadRequest();

        return await SaveTopicAsync(model, cancellationToken);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTopic(
        int id,
        CancellationToken cancellationToken)
    {
        await _versionService.EnsureDraftAsync(cancellationToken);

        try
        {
            await _topicManagementService.DeleteAsync(
                id,
                cancellationToken);

            TempData["AdminMessage"] = "TopicDeleted";
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> SaveTopicAsync(
        AdminTopicEditViewModel model,
        CancellationToken cancellationToken)
    {
        var cultures = GetSupportedCultures();

        if (!ModelState.IsValid)
        {
            model = await BuildViewModelAsync(
                model.Topic,
                cultures,
                cancellationToken);

            return View("EditTopic", model);
        }

        try
        {
            var saveDto = new TopicSaveDto
            {
                Id = model.Topic.Id,
                PositionX = model.Topic.PositionX,
                PositionY = model.Topic.PositionY,
                Icon = model.Topic.Icon,
                TitleTranslations = model.Topic.TitleTranslations,
                DescriptionTranslations = model.Topic.DescriptionTranslations,
                PrerequisiteTopicIds = model.Topic.PrerequisiteTopicIds,
                ContentItems = model.Topic.ContentItems
                    .Select(item => new TopicContentItemSaveDto
                    {
                        Id = item.Id,
                        Type = item.Type,
                        VideoUrl = item.VideoUrl,
                        Translations = item.Translations
                    })
                    .ToList()
            };

            var topicId = await _topicManagementService.SaveAsync(
                saveDto,
                cultures,
                cancellationToken);

            TempData["AdminMessage"] = "TopicSaved";

            return RedirectToAction(nameof(EditTopic), new { id = topicId });
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);

            model = await BuildViewModelAsync(
                model.Topic,
                cultures,
                cancellationToken);

            return View("EditTopic", model);
        }
    }

    private async Task<AdminTopicEditViewModel> BuildViewModelAsync(
        TopicEditorDto topic,
        IReadOnlyList<string> cultures,
        CancellationToken cancellationToken)
    {
        var availableTopics = await _topicQueryService.GetAllAsync(
            CultureInfo.CurrentUICulture.Name,
            cancellationToken,
            ContentVersionStatus.Draft);

        return new AdminTopicEditViewModel
        {
            Topic = topic,
            SupportedCultures = cultures,
            AvailableTopics = availableTopics
                .Where(t => t.Id != topic.Id)
                .ToList()
        };
    }

    private IReadOnlyList<string> GetSupportedCultures()
    {
        return _localizationOptions.SupportedUICultures?
            .Select(c => c.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray() ?? [];
    }

    [HttpGet]
    public async Task<IActionResult> EditTest(
        int topicId,
        CancellationToken cancellationToken)
    {
        await _versionService.EnsureDraftAsync(cancellationToken);

        var cultures = GetSupportedCultures();

        var test = await _testManagementService.GetForEditAsync(
            topicId,
            cultures,
            cancellationToken);

        if (test is null)
            return NotFound();

        return View(new AdminTestEditViewModel
        {
            Test = test,
            SupportedCultures = cultures
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTest(
        int topicId,
        AdminTestEditViewModel model,
        CancellationToken cancellationToken)
    {
        await _versionService.EnsureDraftAsync(cancellationToken);

        if (model.Test.TopicId != topicId)
            return BadRequest();

        var cultures = GetSupportedCultures();
        model.SupportedCultures = cultures;

        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var dto = new TestSaveDto
            {
                TopicId = model.Test.TopicId,
                Questions = model.Test.Questions.Select(question =>
                    new QuestionSaveDto
                    {
                        Id = question.Id,
                        Type = question.Type,
                        Translations = question.Translations,
                        Options = question.Options.Select(option =>
                            new QuestionOptionSaveDto
                            {
                                Id = option.Id,
                                IsCorrect = option.IsCorrect,
                                Translations = option.Translations
                            }).ToList(),
                        CorrectAnswer = question.CorrectAnswer,
                        Tolerance = question.Tolerance,
                        CorrectX = question.CorrectX,
                        CorrectY = question.CorrectY,
                        MinX = question.MinX,
                        MaxX = question.MaxX,
                        MinY = question.MinY,
                        MaxY = question.MaxY,
                        GridStep = question.GridStep
                    }).ToList()
            };

            await _testManagementService.SaveAsync(
                dto,
                cultures,
                cancellationToken);

            TempData["AdminMessage"] = "TestSaved";

            return RedirectToAction(nameof(EditTest), new { topicId });
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTest(
        int topicId,
        CancellationToken cancellationToken)
    {
        await _versionService.EnsureDraftAsync(cancellationToken);

        await _testManagementService.DeleteAsync(
            topicId,
            cancellationToken);

        TempData["AdminMessage"] = "TestDeleted";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Publish(
    CancellationToken cancellationToken)
    {
        await _versionService.EnsureDraftAsync(cancellationToken);

        try
        {
            await _versionService.PublishDraftAsync(cancellationToken);
            TempData["AdminMessage"] = "VersionPublished";
        }
        catch (InvalidOperationException)
        {
            TempData["AdminMessage"] = "PublishFailed";
        }

        return RedirectToAction(nameof(Index));
    }
}
