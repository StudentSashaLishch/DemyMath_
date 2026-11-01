namespace DemyMath.Domain.Entities;

public class NumericQuestion : Question
{
    public decimal CorrectAnswer { get; set; }

    public decimal Tolerance { get; set; }
}