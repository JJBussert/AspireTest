using Aspire.Hosting.Testing;
using JJBussert.Aspire.Domain;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using Xunit.Abstractions;

namespace JJBussert.Aspire.Test;

[Trait("Category", "Authentication")]
public class AuthenticationTests : IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    private DistributedApplicationTestingBuilder? _appHost;
    private DistributedApplication? _app;
    private HttpClient? _apiClient;

    public AuthenticationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    public async Task InitializeAsync()
    {
        _appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.JJBussert_Aspire_AppHost>();
        _app = await _appHost.BuildAsync();
        await _app.StartAsync();
        _apiClient = _app.CreateHttpClient("api");
        
        _output.WriteLine("Authentication test setup completed");
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
    public async Task UnauthorizedUser_AccessingUsers_Returns401()
    {
        // Arrange - Create client without authentication
        var unauthenticatedClient = _app!.CreateHttpClient("api");

        // Act
        var response = await unauthenticatedClient.GetAsync("/api/users");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        _output.WriteLine("✅ Unauthorized access correctly returns 401");
    }

    [Fact]
    public async Task UnauthorizedUser_AccessingOrganizations_Returns401()
    {
        // Arrange - Create client without authentication
        var unauthenticatedClient = _app!.CreateHttpClient("api");

        // Act
        var response = await unauthenticatedClient.GetAsync("/api/organizations");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        _output.WriteLine("✅ Unauthorized access to organizations correctly returns 401");
    }

    [Fact]
    public async Task BasicUser_CanReadUsers_Returns200()
    {
        // Act
        var response = await _apiClient!.GetAsync("/api/users?testUser=basic");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var users = await response.Content.ReadFromJsonAsync<List<User>>();
        Assert.NotNull(users);
        Assert.NotEmpty(users);
        
        _output.WriteLine($"✅ Basic user can read users - Found {users.Count} users");
    }

    [Fact]
    public async Task BasicUser_CanReadOrganizations_Returns200()
    {
        // Act
        var response = await _apiClient!.GetAsync("/api/organizations?testUser=basic");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var organizations = await response.Content.ReadFromJsonAsync<List<Organization>>();
        Assert.NotNull(organizations);
        Assert.NotEmpty(organizations);
        Assert.Equal(10, organizations.Count); // Should have 10 seeded organizations
        
        _output.WriteLine($"✅ Basic user can read organizations - Found {organizations.Count} organizations");
    }

    [Fact]
    public async Task AdminUser_CanReadUsers_Returns200()
    {
        // Act
        var response = await _apiClient!.GetAsync("/api/users?testUser=admin");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var users = await response.Content.ReadFromJsonAsync<List<User>>();
        Assert.NotNull(users);
        Assert.NotEmpty(users);
        
        _output.WriteLine($"✅ Admin user can read users - Found {users.Count} users");
    }

    [Fact]
    public async Task AdminUser_CanReadOrganizations_Returns200()
    {
        // Act
        var response = await _apiClient!.GetAsync("/api/organizations?testUser=admin");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var organizations = await response.Content.ReadFromJsonAsync<List<Organization>>();
        Assert.NotNull(organizations);
        Assert.NotEmpty(organizations);
        
        _output.WriteLine($"✅ Admin user can read organizations - Found {organizations.Count} organizations");
    }

    [Fact]
    public async Task BasicUser_CannotCreateUser_Returns403()
    {
        // Arrange
        var organizations = await GetOrganizationsAsync();
        var newUser = new User
        {
            Name = "Test User",
            Email = "testuser@example.com",
            Role = "Basic",
            OrganizationId = organizations.First().Id
        };

        // Act
        var response = await _apiClient!.PostAsJsonAsync("/api/users?testUser=basic", newUser);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        _output.WriteLine("✅ Basic user correctly forbidden from creating users");
    }

    [Fact]
    public async Task BasicUser_CannotCreateOrganization_Returns403()
    {
        // Arrange
        var newOrg = new Organization
        {
            Name = "Test Organization",
            Description = "Test Description"
        };

        // Act
        var response = await _apiClient!.PostAsJsonAsync("/api/organizations?testUser=basic", newOrg);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        _output.WriteLine("✅ Basic user correctly forbidden from creating organizations");
    }

    [Fact]
    public async Task AdminUser_CanCreateUser_Returns201()
    {
        // Arrange
        var organizations = await GetOrganizationsAsync();
        var newUser = new User
        {
            Name = "Admin Created User",
            Email = "admincreated@example.com",
            Role = "Basic",
            OrganizationId = organizations.First().Id
        };

        // Act
        var response = await _apiClient!.PostAsJsonAsync("/api/users?testUser=admin", newUser);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var createdUser = await response.Content.ReadFromJsonAsync<User>();
        Assert.NotNull(createdUser);
        Assert.Equal(newUser.Name, createdUser.Name);
        Assert.Equal(newUser.Email, createdUser.Email);
        
        _output.WriteLine($"✅ Admin user successfully created user: {createdUser.Name}");
    }

    [Fact]
    public async Task AdminUser_CanCreateOrganization_Returns201()
    {
        // Arrange
        var newOrg = new Organization
        {
            Name = "Admin Created Organization",
            Description = "Created by admin user"
        };

        // Act
        var response = await _apiClient!.PostAsJsonAsync("/api/organizations?testUser=admin", newOrg);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var createdOrg = await response.Content.ReadFromJsonAsync<Organization>();
        Assert.NotNull(createdOrg);
        Assert.Equal(newOrg.Name, createdOrg.Name);
        Assert.Equal(newOrg.Description, createdOrg.Description);
        
        _output.WriteLine($"✅ Admin user successfully created organization: {createdOrg.Name}");
    }

    [Fact]
    public async Task TestUsers_ExistInDatabase()
    {
        // Act
        var response = await _apiClient!.GetAsync("/api/users?testUser=admin");
        var users = await response.Content.ReadFromJsonAsync<List<User>>();

        // Assert
        Assert.NotNull(users);
        
        var adminUser = users.FirstOrDefault(u => u.Email == "admin@test.com");
        var basicUser = users.FirstOrDefault(u => u.Email == "basic@test.com");
        
        Assert.NotNull(adminUser);
        Assert.Equal("Admin", adminUser.Role);
        
        Assert.NotNull(basicUser);
        Assert.Equal("Basic", basicUser.Role);
        
        _output.WriteLine("✅ Test users found in database:");
        _output.WriteLine($"  - Admin: {adminUser.Name} ({adminUser.Email})");
        _output.WriteLine($"  - Basic: {basicUser.Name} ({basicUser.Email})");
    }

    [Theory]
    [InlineData("admin", HttpStatusCode.OK)]
    [InlineData("basic", HttpStatusCode.OK)]
    [InlineData("", HttpStatusCode.Unauthorized)]
    public async Task UserEndpoint_ReturnsCorrectStatusForDifferentUsers(string testUser, HttpStatusCode expectedStatus)
    {
        // Arrange
        var url = string.IsNullOrEmpty(testUser) ? "/api/users" : $"/api/users?testUser={testUser}";
        var client = string.IsNullOrEmpty(testUser) ? _app!.CreateHttpClient("api") : _apiClient!;

        // Act
        var response = await client.GetAsync(url);

        // Assert
        Assert.Equal(expectedStatus, response.StatusCode);
        _output.WriteLine($"✅ User '{testUser}' access test - Expected: {expectedStatus}, Got: {response.StatusCode}");
    }

    [Theory]
    [InlineData("admin", HttpStatusCode.Created)]
    [InlineData("basic", HttpStatusCode.Forbidden)]
    public async Task CreateUser_ReturnsCorrectStatusForDifferentRoles(string testUser, HttpStatusCode expectedStatus)
    {
        // Arrange
        var organizations = await GetOrganizationsAsync();
        var newUser = new User
        {
            Name = $"Test User {Guid.NewGuid():N}",
            Email = $"test{Guid.NewGuid():N}@example.com",
            Role = "Basic",
            OrganizationId = organizations.First().Id
        };

        // Act
        var response = await _apiClient!.PostAsJsonAsync($"/api/users?testUser={testUser}", newUser);

        // Assert
        Assert.Equal(expectedStatus, response.StatusCode);
        _output.WriteLine($"✅ Create user test for '{testUser}' - Expected: {expectedStatus}, Got: {response.StatusCode}");
    }

    private async Task<List<Organization>> GetOrganizationsAsync()
    {
        var response = await _apiClient!.GetAsync("/api/organizations?testUser=admin");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<Organization>>() ?? new List<Organization>();
    }
}
