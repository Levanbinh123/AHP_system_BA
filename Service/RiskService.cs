namespace DSSStudentRisk.Service;
public class RiskService
{

    public double CalculateRisk(
        double test,
        double attendance,
        double study,
        double wTest,
        double wAttendance,
        double wStudy)
    {
        double rTest = 1 - (test / 10);
        double rAttendance = 1 - (attendance / 100);
        double rStudy = study < 2 ? 1 
                : study < 4 ? 0.7 
                : study < 6 ? 0.4 
                : 0.1;
        return (rTest * wTest)
            + (rAttendance * wAttendance)
            + (rStudy * wStudy);
    }
}