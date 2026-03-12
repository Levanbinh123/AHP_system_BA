using System.Text.Json.Serialization;

namespace DSSStudentRisk.Models;
public class StudentPerformance
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public double TestScore { get; set; }

    public double Attendance { get; set; }

    public double StudyHours { get; set; }

    public DateTime CreatedDate { get; set; }
     [JsonIgnore]
     public Student? Student { get; set; }
}