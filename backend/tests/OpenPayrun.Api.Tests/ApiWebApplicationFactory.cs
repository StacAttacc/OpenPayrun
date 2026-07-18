using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace OpenPayrun.Api.Tests;

[CollectionDefinition("Api")]
public class ApiCollection : ICollectionFixture<ApiWebApplicationFactory> { }

public class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Admin:Username"] = "testadmin",
                ["Admin:Password"] = "testpass",
                ["Jwt:Secret"] = "test-secret-32-chars-minimum-ok!!",
                // Separate test DB so tests don't touch dev data
                ["ConnectionStrings:Default"] =
                    "Server=mssql,1433;Database=OpenPayrunTests;User Id=sa;Password=DevPassword123!;TrustServerCertificate=True;",
            }));
    }
}
