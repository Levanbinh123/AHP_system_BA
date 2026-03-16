using System.Text.Json.Serialization;
using DSSStudentRisk.Models;
namespace DSSStudentRisk.Models;
public class AhpFinalResult
{
   

    public int Id { get; set; }
    public double A1 { get; set; }
    public double A2 { get; set; }
    public double A3 { get; set; }

    public string BestAlternative { get; set; }
     public int AHPCriteriaId { get; set; }
    [JsonIgnore]  
    public AHPCriteria? AHPCriteria { get; set; }

    public DateTime CreatedDate { get; set; }
    
}