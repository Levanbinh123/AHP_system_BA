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

   public string GetLevel(double score, double A1, double A2)
        {
            if (score <= A1)
                return "Low Risk";
            else if (score <= A2)
                return "Medium Risk";
            else
                return "High Risk";
        }
}