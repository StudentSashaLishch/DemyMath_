using DemyMath.Application.Tests.Models;

namespace DemyMath.Models;

public class AdminTestEditViewModel
{
    public required TestEditorDto Test { get; init; }
    public IReadOnlyList<string> SupportedCultures { get; set; } = [];
}