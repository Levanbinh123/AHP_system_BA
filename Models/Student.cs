namespace DSSStudentRisk.Models;
public class Student
{
    public int Id { get; set; }
    public string StudentCode{get;set;}

    public string? Name { get; set; }
    public string? ClassName{get;set;}
    public List<StudentPerformance>? Performances { get; set; }

    public string? Email { get; set; }
}