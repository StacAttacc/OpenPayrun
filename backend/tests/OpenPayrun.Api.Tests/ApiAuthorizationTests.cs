using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace OpenPayrun.Api.Tests;

[Collection("Api")]
public class ApiAuthorizationTests(ApiWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetTaxRates_NoToken_Returns200()
    {
        var resp = await _client.GetAsync("/api/tax-rates");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task PostTaxRates_NoToken_Returns401()
    {
        var resp = await _client.PostAsJsonAsync("/api/tax-rates", new { });
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task PutTaxRate_NoToken_Returns401()
    {
        var resp = await _client.PutAsJsonAsync("/api/tax-rates/1", new { });
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task PostTaxRates_WithToken_Returns201()
    {
        var token = await GetTokenAsync();
        var req = AuthorizedPost("/api/tax-rates", ValidRateSetBody("2027-01-01"), token);
        var resp = await _client.SendAsync(req);
        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
    }

    [Fact]
    public async Task PutTaxRate_WithToken_Returns200()
    {
        var token = await GetTokenAsync();

        var createResp = await _client.SendAsync(
            AuthorizedPost("/api/tax-rates", ValidRateSetBody("2028-01-01"), token));
        var created = await createResp.Content.ReadFromJsonAsync<CreatedResponse>();

        var updateResp = await _client.SendAsync(
            AuthorizedPut($"/api/tax-rates/{created!.Id}", ValidRateSetBody("2028-01-01"), token));
        Assert.Equal(HttpStatusCode.OK, updateResp.StatusCode);
    }

    private async Task<string> GetTokenAsync()
    {
        var resp = await _client.PostAsJsonAsync("/api/auth/login",
            new { username = "testadmin", password = "testpass" });
        var body = await resp.Content.ReadFromJsonAsync<LoginResponse>();
        return body!.Token;
    }

    private static HttpRequestMessage AuthorizedPost(string uri, object body, string token) =>
        new(HttpMethod.Post, uri)
        {
            Content = JsonContent.Create(body),
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) }
        };

    private static HttpRequestMessage AuthorizedPut(string uri, object body, string token) =>
        new(HttpMethod.Put, uri)
        {
            Content = JsonContent.Create(body),
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) }
        };

    private static object ValidRateSetBody(string effectiveFrom) => new
    {
        effectiveFrom,
        effectiveTo = (string?)null,
        qppExemption = 3500m, qppYmpe = 74600m, qppYampe = 85000m,
        qppBaseRate = 0.053m, qppAdditionalTier1Rate = 0.01m, qppTier1Rate = 0.063m, qppTier2Rate = 0.04m,
        qppTier1MaxEmployee = 4479.30m, qppTier2MaxEmployee = 416.00m,
        eiEmployeeRate = 0.013m, eiEmployerMultiplier = 1.4m,
        eiMaxInsurableEarnings = 68900m, eiMaxEmployeePremium = 895.70m,
        qpipEmployeeRate = 0.0043m, qpipEmployerRate = 0.00602m,
        qpipMaxInsurableEarnings = 103000m, qpipMaxEmployeePremium = 442.90m, qpipMaxEmployerPremium = 620.06m,
        federalBasicPersonalAmount = 16452m, federalEmploymentAmount = 1501m,
        federalLowestRate = 0.14m, quebecFederalAbatement = 0.165m,
        quebecBasicPersonalAmount = 18952m, quebecWorkerDeductionMax = 1450m,
        quebecWorkerDeductionRate = 0.06m, quebecLowestRate = 0.14m,
        fssqSmallEmployerRate = 0.0165m, fssqLargeEmployerRate = 0.0426m,
        federalBrackets = new[]
        {
            new { upperBound = (decimal?)58523m, rate = 0.14m },
            new { upperBound = (decimal?)117045m, rate = 0.205m },
            new { upperBound = (decimal?)181440m, rate = 0.26m },
            new { upperBound = (decimal?)258482m, rate = 0.29m },
            new { upperBound = (decimal?)null,    rate = 0.33m },
        },
        quebecBrackets = new[]
        {
            new { upperBound = (decimal?)54345m, rate = 0.14m },
            new { upperBound = (decimal?)108680m, rate = 0.19m },
            new { upperBound = (decimal?)132245m, rate = 0.24m },
            new { upperBound = (decimal?)null,    rate = 0.2575m },
        },
    };

    private record LoginResponse(string Token);
    private record CreatedResponse(int Id);
}
