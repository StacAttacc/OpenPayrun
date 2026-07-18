using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace OpenPayrun.Api.Tests;

[Collection("Api")]
public class AuthTests(ApiWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Login_ValidCredentials_Returns200WithToken()
    {
        var resp = await _client.PostAsJsonAsync("/api/auth/login",
            new { username = "testadmin", password = "testpass" });

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var body = await resp.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.False(string.IsNullOrEmpty(body?.Token));
    }

    [Theory]
    [InlineData("testadmin", "wrongpass")]
    [InlineData("wronguser", "testpass")]
    [InlineData("", "")]
    public async Task Login_InvalidCredentials_Returns401(string username, string password)
    {
        var resp = await _client.PostAsJsonAsync("/api/auth/login",
            new { username, password });

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    private record LoginResponse(string Token);
}
