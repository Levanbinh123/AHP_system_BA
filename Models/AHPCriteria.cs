namespace DSSStudentRisk.Models;
public class AHPCriteria
{
    public int Id{ get; set; }

    public double Test_Attendance { get; set; }

    public double Test_Study { get; set; }

    public double Attendance_Study { get; set; }

    public double TestWeight { get; set; }

    public double AttendanceWeight { get; set; }

    public double StudyWeight { get; set; }

    public double ConsistencyRatio { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }
    public List<AHPAlternativeWeight>? Alternatives { get; set; }
    public List<AhpFinalResult>? FinalResults { get; set; }
}