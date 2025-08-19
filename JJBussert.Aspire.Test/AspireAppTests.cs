using Aspire.Hosting.Testing;
using JJBussert.Aspire.Domain;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace JJBussert.Aspire.Test;

public class AspireAppTests : IAsyncLifetime
{
    private DistributedApplicationTestingBuilder? _appHost;
    private DistributedApplication? _app;
    private HttpClient? _apiClient;

    public async Task InitializeAsync()
    {
        // Create the distributed application testing builder
        _appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.JJBussert_Aspire_AppHost>();
        
        // Build and start the application
        _app = await _appHost.BuildAsync();
        await _app.StartAsync();

        // Get the API client
        _apiClient = _app.CreateHttpClient("api");
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
    public async Task Api_GetUsers_WithBasicUser_ReturnsUsers()
    {
        // Arrange & Act
        var response = await _apiClient!.GetAsync("/api/users?testUser=basic");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var users = await response.Content.ReadFromJsonAsync<List<User>>();
        Assert.NotNull(users);
        Assert.NotEmpty(users);
    }

    [Fact]
    public async Task Api_GetUsers_WithAdminUser_ReturnsUsers()
    {
        // Arrange & Act
        var response = await _apiClient!.GetAsync("/api/users?testUser=admin");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var users = await response.Content.ReadFromJsonAsync<List<User>>();
        Assert.NotNull(users);
        Assert.NotEmpty(users);
    }

    [Fact]
    public async Task Api_GetOrganizations_WithAuthenticatedUser_ReturnsOrganizations()
    {
        // Arrange & Act
        var response = await _apiClient!.GetAsync("/api/organizations?testUser=basic");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var organizations = await response.Content.ReadFromJsonAsync<List<Organization>>();
        Assert.NotNull(organizations);
        Assert.NotEmpty(organizations);
        Assert.Equal(10, organizations.Count); // We seed 10 organizations
    }

    [Fact]
    public async Task Api_GetUsers_ContainsTestUsers()
    {
        // Arrange & Act
        var response = await _apiClient!.GetAsync("/api/users?testUser=admin");
        var users = await response.Content.ReadFromJsonAsync<List<User>>();

        // Assert
        Assert.NotNull(users);

        var adminUser = users.FirstOrDefault(u => u.Email == "admin@test.com");
        Assert.NotNull(adminUser);
        Assert.Equal("Admin", adminUser.Role);

        var basicUser = users.FirstOrDefault(u => u.Email == "basic@test.com");
        Assert.NotNull(basicUser);
        Assert.Equal("Basic", basicUser.Role);
    }

    [Fact]
    public async Task Api_GetUsers_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange - Create a new client without authentication headers
        var unauthenticatedClient = _app!.CreateHttpClient("api");

        // Act
        var response = await unauthenticatedClient.GetAsync("/api/users");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Api_CreateUser_WithBasicUser_ReturnsForbidden()
    {
        // Arrange
        var organizations = await GetOrganizationsAsync();
        var firstOrg = organizations.First();

        var newUser = new User
        {
            Name = "Test User",
            Email = "testuser@example.com",
            Role = "Basic",
            OrganizationId = firstOrg.Id
        };

        // Act
        var response = await _apiClient!.PostAsJsonAsync("/api/users?testUser=basic", newUser);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Api_CreateUser_WithAdminUser_ReturnsCreated()
    {
        // Arrange
        var organizations = await GetOrganizationsAsync();
        var firstOrg = organizations.First();

        var newUser = new User
        {
            Name = "Test User",
            Email = "testuser@example.com",
            Role = "Basic",
            OrganizationId = firstOrg.Id
        };

        // Act
        var response = await _apiClient!.PostAsJsonAsync("/api/users?testUser=admin", newUser);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdUser = await response.Content.ReadFromJsonAsync<User>();
        Assert.NotNull(createdUser);
        Assert.Equal(newUser.Name, createdUser.Name);
        Assert.Equal(newUser.Email, createdUser.Email);
        Assert.Equal(newUser.Role, createdUser.Role);
    }

    [Fact]
    public async Task Api_GetNonExistentUser_ReturnsNotFound()
    {
        // Arrange & Act
        var response = await _apiClient!.GetAsync("/api/users/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<List<Organization>> GetOrganizationsAsync()
    {
        var response = await _apiClient!.GetAsync("/api/organizations?testUser=admin");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<Organization>>() ?? new List<Organization>();
    }
}
