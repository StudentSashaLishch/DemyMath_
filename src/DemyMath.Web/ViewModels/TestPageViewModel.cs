using DemyMath.Application.Tests.Models;

namespace DemyMath.ViewModels;

public class TestPageViewModel
{
    public required TestDto Test { get; init; }
    public TestResultDto? Result { get; init; }
    public TestSubmissionDto Submission { get; init; } = new();
}