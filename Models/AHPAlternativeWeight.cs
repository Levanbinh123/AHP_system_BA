using System.Text.Json.Serialization;

namespace DSSStudentRisk.Models;

public class AHPAlternativeWeight
{
    public int Id { get; set; }

    public string CriteriaName { get; set; }

    public double A1 { get; set; }

    public double A2 { get; set; }

    public double A3 { get; set; }

    public int AHPCriteriaId { get; set; }
 [JsonIgnore]
    public AHPCriteria? AHPCriteria { get; set; }

    public DateTime CreatedDate { get; set; }
}