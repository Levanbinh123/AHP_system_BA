namespace DSSStudentRisk.Models;
public class RiskResult
{
    public int Id { get; set; }  

    public int StudentId { get; set; }

    public double RiskScore { get; set; }

    public string? RiskLevel { get; set; }

    public DateTime CalculatedDate { get; set; }
    public Student? Student { get; set; }
}