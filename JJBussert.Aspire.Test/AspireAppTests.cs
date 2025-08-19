using Aspire.Hosting.Testing;
using JJBussert.Aspire.Domain;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace JJBussert.Aspire.Test;

[Trait("Category", "Integration")]
public class AspireAppTests : IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    private DistributedApplicationTestingBuilder? _appHost;
    private DistributedApplication? _app;
    private HttpClient? _apiClient;

    public AspireAppTests(ITestOutputHelper output)
    {
        _output = output;
    }

    public async Task InitializeAsync()
    {
        // Create the distributed application testing builder
        _appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.JJBussert_Aspire_AppHost>();

        // Build and start the application
        _app = await _appHost.BuildAsync();
        await _app.StartAsync();

        // Get the API client
        _apiClient = _app.CreateHttpClient("api");

        _output.WriteLine("Aspire application started successfully");
        _output.WriteLine($"API endpoint: {_apiClient.BaseAddress}");
    }

    public async Task DisposeAsync()
    {
        _apiClient?.Dispose();
        if (_app is not null)
        {
            await _app.DisposeAsync();
        }
        _appHost?.Dispose();
    }

    [Fact]
    public async Task Api_HealthCheck_ReturnsHealthy()
    {
        // Arrange & Act
        var response = await _apiClient!.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Api_GetUsers_WithAuthentication_ReturnsUsers()
    {
        // Arrange & Act
        var response = await _apiClient!.GetAsync("/api/users?testUser=basic");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var users = await response.Content.ReadFromJsonAsync<List<User>>();
        Assert.NotNull(users);
        Assert.NotEmpty(users);

        _output.WriteLine($"API returned {users.Count} users successfully");
    }

    [Fact]
    public async Task Api_GetOrganizations_WithAuthentication_ReturnsOrganizations()
    {
        // Arrange & Act
        var response = await _apiClient!.GetAsync("/api/organizations?testUser=basic");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var organizations = await response.Content.ReadFromJsonAsync<List<Organization>>();
        Assert.NotNull(organizations);
        Assert.NotEmpty(organizations);
        Assert.Equal(10, organizations.Count); // We seed 10 organizations

        _output.WriteLine($"API returned {organizations.Count} organizations successfully");
    }

    [Fact]
    public async Task Api_GetNonExistentUser_ReturnsNotFound()
    {
        // Arrange & Act
        var response = await _apiClient!.GetAsync("/api/users/99999?testUser=admin");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        _output.WriteLine("Non-existent user correctly returns 404");
    }

    [Fact]
    public async Task AspireOrchestration_AllServicesHealthy()
    {
        // Test that all Aspire-orchestrated services are healthy
        var healthResponse = await _apiClient!.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, healthResponse.StatusCode);

        var healthContent = await healthResponse.Content.ReadAsStringAsync();
        _output.WriteLine($"Health check response: {healthContent}");

        // Test that we can reach both API endpoints (indicating full stack is working)
        var usersResponse = await _apiClient!.GetAsync("/api/users?testUser=basic");
        var orgsResponse = await _apiClient!.GetAsync("/api/organizations?testUser=basic");

        Assert.Equal(HttpStatusCode.OK, usersResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, orgsResponse.StatusCode);

        _output.WriteLine("✅ All Aspire services are healthy and responding");
    }

    private async Task<List<Organization>> GetOrganizationsAsync()
    {
        var response = await _apiClient!.GetAsync("/api/organizations?testUser=admin");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<Organization>>() ?? new List<Organization>();
    }
}
}
