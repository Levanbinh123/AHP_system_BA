namespace DSSStudentRisk.Service;
public class AHPService
{

    public (double test, double attendance, double study, double cr)
        Calculate(double ta, double ts, double as_)
    {

        double[,] matrix =
        {
            {1, ta, ts},
            {1/ta, 1, as_},
            {1/ts, 1/as_, 1}
        };

        int n = 3;

        double[] colSum = new double[n];

        for (int j = 0; j < n; j++)
            for (int i = 0; i < n; i++)
                colSum[j] += matrix[i, j];

        double[,] norm = new double[n, n];

        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                norm[i, j] = matrix[i, j] / colSum[j];

        double[] weight = new double[n];

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
                weight[i] += norm[i, j];

            weight[i] /= n;
        }

        double lambdaMax = 0;

        for (int i = 0; i < n; i++)
        {
            double sum = 0;

            for (int j = 0; j < n; j++)
                sum += matrix[i, j] * weight[j];

            lambdaMax += sum / weight[i];
        }

        lambdaMax /= n;

        double CI = (lambdaMax - n) / (n - 1);

        double RI = 0.58;

        double CR = CI / RI;

        return (weight[0], weight[1], weight[2], CR);
    }
}