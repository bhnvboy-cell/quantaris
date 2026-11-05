using System.Security.Cryptography;
using System.Text;
using Quantaris.Api.Controllers;
using Quantaris.Infrastructure.Data;
using Quantaris.Infrastructure.Services;

namespace Quantaris.Tests;

public class ProcessCapabilityTests
{
    // Cp = (USL - LSL) / (6 * sigma)
    [Theory]
    [InlineData(10, 0, 1.0, 1.6667)]  // Cp = 10 / 6 = 1.6667
    [InlineData(6, 0, 1.0, 1.0)]
    [InlineData(12, 0, 1.0, 2.0)]
    public void Cp_Calculation(double usl, double lsl, double sigma, double expected)
    {
        double cp = (usl - lsl) / (6 * sigma);
        Assert.True(Math.Abs(cp - expected) < 0.001);
    }

    // Cpk = min((USL - mean) / (3*sigma), (mean - LSL) / (3*sigma))
    [Theory]
    [InlineData(10, 0, 5.0, 1.0, 1.6667)]  // symmetric
    [InlineData(10, 0, 6.0, 1.0, 1.3333)]  // off-center
    public void Cpk_Calculation(double usl, double lsl, double mean, double sigma, double expected)
    {
        double cpk = Math.Min((usl - mean) / (3 * sigma), (mean - lsl) / (3 * sigma));
        Assert.True(Math.Abs(cpk - expected) < 0.001);
    }

    // DPMO from Z-score
    [Fact]
    public void DPMO_From_ZScore_3sigma()
    {
        double z = 3.0;
        double p = 0.5 * (1 + Erf(z / Math.Sqrt(2)));
        double defectRate = 1 - p;
        int dpmo = (int)Math.Round(defectRate * 1_000_000);
        Assert.Equal(1350, dpmo);
    }

    [Fact]
    public void DPMO_From_ZScore_4sigma()
    {
        double z = 4.0;
        double p = 0.5 * (1 + Erf(z / Math.Sqrt(2)));
        double defectRate = 1 - p;
        int dpmo = (int)Math.Round(defectRate * 1_000_000);
        Assert.Equal(32, dpmo);
    }

    [Fact]
    public void DPMO_From_ZScore_6sigma()
    {
        double z = 6.0;
        double p = 0.5 * (1 + Erf(z / Math.Sqrt(2)));
        double defectRate = 1 - p;
        int dpmo = (int)Math.Round(defectRate * 1_000_000);
        Assert.Equal(0, dpmo);
    }

    private static double Erf(double x)
    {
        double t = 1.0 / (1.0 + 0.3275911 * Math.Abs(x));
        double poly = t * (0.254829592 + t * (-0.284496736 + t * (1.421413741 + t * (-1.453152027 + t * 1.061405429))));
        double result = 1.0 - poly * Math.Exp(-x * x);
        return x >= 0 ? result : -result;
    }
}

public class ControlChartTests
{
    // I-MR Chart: Center Line = X-bar
    [Fact]
    public void IMR_CenterLine_IsMean()
    {
        double[] data = { 10, 12, 11, 13, 10, 12, 11 };
        double mean = data.Average();
        Assert.Equal(11.2857, mean, 4);
    }

    // Moving Range
    [Fact]
    public void MovingRange_Calculation()
    {
        double[] data = { 10, 12, 11, 13, 10 };
        var mr = new List<double>();
        for (int i = 1; i < data.Length; i++)
            mr.Add(Math.Abs(data[i] - data[i - 1]));

        Assert.Equal(new double[] { 2, 1, 2, 3 }, mr.ToArray());
    }

    // UCL = X-bar + 2.66 * MR-bar (for I-MR chart)
    [Fact]
    public void IMR_UCL_Calculation()
    {
        double[] data = { 10, 12, 11, 13, 10, 12, 11 };
        double xBar = data.Average();
        var mr = new List<double>();
        for (int i = 1; i < data.Length; i++)
            mr.Add(Math.Abs(data[i] - data[i - 1]));
        double mrBar = mr.Average();
        double ucl = xBar + 2.66 * mrBar;
        double lcl = xBar - 2.66 * mrBar;

        Assert.True(ucl > xBar);
        Assert.True(lcl < xBar);
    }
}

public class DmaicStepTests
{
    [Theory]
    [InlineData(1, "Define")]
    [InlineData(3, "Define")]
    [InlineData(4, "Measure")]
    [InlineData(6, "Measure")]
    [InlineData(7, "Analyze")]
    [InlineData(9, "Analyze")]
    [InlineData(10, "Improve")]
    [InlineData(12, "Improve")]
    [InlineData(13, "Control")]
    [InlineData(15, "Control")]
    [InlineData(99, "Unknown")]
    public void GetPhaseForStep_ReturnsCorrectPhase(int step, string expectedPhase)
    {
        var phase = DmaicService.GetPhaseForStep(step);
        Assert.Equal(expectedPhase, phase);
    }

    [Theory]
    [InlineData(1, "Understand Customer & Business Requirements")]
    [InlineData(8, "Ascertain Critical Root Causes")]
    [InlineData(15, "Celebrate and Recognize Success")]
    [InlineData(0, "Unknown Step")]
    public void GetStepName_ReturnsExpectedName(int step, string expectedName)
    {
        var name = DmaicService.GetStepName(step);
        Assert.Equal(expectedName, name);
    }

    [Fact]
    public void FMEA_RPN_Calculation()
    {
        int severity = 8, occurrence = 4, detection = 3;
        int rpn = severity * occurrence * detection;
        Assert.Equal(96, rpn);
    }

    [Fact]
    public void FMEA_NewRPN_WithReducedValues()
    {
        int severity = 8, occurrence = 2, detection = 2;
        int newRpn = severity * occurrence * detection;
        Assert.Equal(32, newRpn);
        Assert.True(newRpn < 96);
    }
}

public class AuthAndTenancyTests
{
    [Fact]
    public void HashAndVerifyPassword_Pbkdf2WithStoredSalt_Succeeds()
    {
        var salt = AuthController.GenerateSalt();
        var hash = AuthController.HashPassword("StrongP@ss123!", salt);

        Assert.True(AuthController.VerifyPassword("StrongP@ss123!", hash, salt));
        Assert.False(AuthController.VerifyPassword("WrongPassword", hash, salt));
    }

    [Fact]
    public void VerifyPassword_LegacySaltedSha256_Succeeds()
    {
        var salt = "demosalt123";
        var combined = "Admin@123" + salt;
        var legacyHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(combined)));

        Assert.True(AuthController.VerifyPassword("Admin@123", legacyHash, salt));
        Assert.False(AuthController.VerifyPassword("WrongPass", legacyHash, salt));
    }

    [Fact]
    public void HttpTenantResolver_IsolatesTenantPerInstance()
    {
        var resolverA = new HttpTenantResolver();
        var resolverB = new HttpTenantResolver();

        resolverA.SetTenant(1, "TENANT_A");
        resolverB.SetTenant(2, "TENANT_B");

        Assert.Equal(1, resolverA.GetCurrentTenantId());
        Assert.Equal("TENANT_A", resolverA.GetCurrentTenantCode());
        Assert.Equal(2, resolverB.GetCurrentTenantId());
        Assert.Equal("TENANT_B", resolverB.GetCurrentTenantCode());
    }
}
