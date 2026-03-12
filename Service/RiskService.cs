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

        double rStudy = 1 - (study / 10);

        return (rTest * wTest)
            + (rAttendance * wAttendance)
            + (rStudy * wStudy);
    }

    public string GetLevel(double score)
    {
        if (score < 0.4)
            return "Low";

        if (score < 0.7)
            return "Medium";

        return "High";
    }
}