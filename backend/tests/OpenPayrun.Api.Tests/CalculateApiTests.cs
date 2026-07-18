using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace OpenPayrun.Api.Tests;

[Collection("Api")]
public class CalculateApiTests
{
    private readonly HttpClient _client;

    public CalculateApiTests(ApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Calculate_NoToken_Returns200()
    {
        var resp = await _client.PostAsJsonAsync("/api/pay-runs/calculate", new
        {
            periodStart = "2026-01-01",
            grossPay = 8333m,
            frequency = 12,
            selfEmployed = true,
            ytdGrossEarnings = 0m,
        });

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task Calculate_SelfEmployedMonth1_ReturnsExcelVerifiedValues()
    {
        var resp = await _client.PostAsJsonAsync("/api/pay-runs/calculate", new
        {
            periodStart = "2026-01-01",
            grossPay = 8333m,
            frequency = 12,
            selfEmployed = true,
            ytdGrossEarnings = 0m,
        });

        var r = await resp.Content.ReadFromJsonAsync<CalcResult>();
        Assert.NotNull(r);
        Assert.Equal(8333m,    r.GrossPay);
        Assert.Equal(506.60m,  r.QppTier1);   // CRA PDOC-verified
        Assert.Equal(0m,       r.EiPremium);  // self-employed = no EI
        Assert.Equal(5760.91m, r.NetPay);     // Excel ground truth
    }

    [Fact]
    public async Task Calculate_NoRateSetForPeriod_Returns400()
    {
        var resp = await _client.PostAsJsonAsync("/api/pay-runs/calculate", new
        {
            periodStart = "2020-01-01",
            grossPay = 5000m,
            frequency = 12,
            selfEmployed = false,
            ytdGrossEarnings = 0m,
        });

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    private record CalcResult(
        decimal GrossPay, decimal QppTier1, decimal QppTier2,
        decimal EiPremium, decimal QpipPremium,
        decimal FederalIncomeTax, decimal QuebecIncomeTax,
        decimal TotalEmployeeDeductions, decimal NetPay,
        decimal EmployerQppTier1, decimal EmployerQppTier2,
        decimal EmployerEi, decimal EmployerQpip, decimal EmployerFssq,
        decimal TotalEmployerContributions);
}
