namespace DemyMath.Domain.Entities;

public class CoordinatePointQuestion : Question
{
    public decimal CorrectX { get; set; }
    public decimal CorrectY { get; set; }

    public decimal Tolerance { get; set; }

    public decimal MinX { get; set; }
    public decimal MaxX { get; set; }

    public decimal MinY { get; set; }
    public decimal MaxY { get; set; }

    public decimal GridStep { get; set; }
}