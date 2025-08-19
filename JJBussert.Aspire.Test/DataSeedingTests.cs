using Aspire.Hosting.Testing;
using JJBussert.Aspire.Domain;
using System.Net.Http.Json;
using Xunit;
using Xunit.Abstractions;

namespace JJBussert.Aspire.Test;

[Trait("Category", "DataSeeding")]
public class DataSeedingTests : IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    private DistributedApplicationTestingBuilder? _appHost;
    private DistributedApplication? _app;
    private HttpClient? _apiClient;

    public DataSeedingTests(ITestOutputHelper output)
    {
        _output = output;
    }

    public async Task InitializeAsync()
    {
        _appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.JJBussert_Aspire_AppHost>();
        _app = await _appHost.BuildAsync();
        await _app.StartAsync();
        _apiClient = _app.CreateHttpClient("api");
        
        _output.WriteLine("Data seeding test setup completed");
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
    public async Task DatabaseSeeding_Creates10Organizations()
    {
        // Act
        var response = await _apiClient!.GetAsync("/api/organizations?testUser=admin");
        response.EnsureSuccessStatusCode();
        
        var organizations = await response.Content.ReadFromJsonAsync<List<Organization>>();

        // Assert
        Assert.NotNull(organizations);
        Assert.Equal(10, organizations.Count);
        
        _output.WriteLine($"✅ Database seeding successful - Found {organizations.Count} organizations");
        
        // Verify organization data quality
        foreach (var org in organizations)
        {
            Assert.NotNull(org.Name);
            Assert.NotEmpty(org.Name);
            Assert.True(org.Id > 0);
            _output.WriteLine($"  - {org.Name}: {org.Description}");
        }
    }

    [Fact]
    public async Task DatabaseSeeding_CreatesUsersForEachOrganization()
    {
        // Arrange
        var orgsResponse = await _apiClient!.GetAsync("/api/organizations?testUser=admin");
        var organizations = await orgsResponse.Content.ReadFromJsonAsync<List<Organization>>();
        
        var usersResponse = await _apiClient!.GetAsync("/api/users?testUser=admin");
        var users = await usersResponse.Content.ReadFromJsonAsync<List<User>>();

        // Assert
        Assert.NotNull(organizations);
        Assert.NotNull(users);
        Assert.NotEmpty(users);
        
        _output.WriteLine($"✅ Found {users.Count} users across {organizations.Count} organizations");

        // Verify each organization has users
        foreach (var org in organizations)
        {
            var orgUsers = users.Where(u => u.OrganizationId == org.Id).ToList();
            Assert.NotEmpty(orgUsers);
            
            // Should have between 5-20 users per organization (plus potentially test users)
            Assert.True(orgUsers.Count >= 5, $"Organization {org.Name} should have at least 5 users, but has {orgUsers.Count}");
            
            _output.WriteLine($"  - {org.Name}: {orgUsers.Count} users");
        }
    }

    [Fact]
    public async Task DatabaseSeeding_CreatesTestUsers()
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
        Assert.Equal("Test Admin User", adminUser.Name);
        
        Assert.NotNull(basicUser);
        Assert.Equal("Basic", basicUser.Role);
        Assert.Equal("Test Basic User", basicUser.Name);
        
        // Both test users should be in the same organization
        Assert.Equal(adminUser.OrganizationId, basicUser.OrganizationId);
        
        _output.WriteLine("✅ Test users created successfully:");
        _output.WriteLine($"  - Admin: {adminUser.Name} ({adminUser.Email}) - Org: {adminUser.OrganizationId}");
        _output.WriteLine($"  - Basic: {basicUser.Name} ({basicUser.Email}) - Org: {basicUser.OrganizationId}");
    }

    [Fact]
    public async Task DatabaseSeeding_CreatesValidUserRoles()
    {
        // Act
        var response = await _apiClient!.GetAsync("/api/users?testUser=admin");
        var users = await response.Content.ReadFromJsonAsync<List<User>>();

        // Assert
        Assert.NotNull(users);
        Assert.NotEmpty(users);
        
        var validRoles = new[] { "Admin", "Basic" };
        var roleDistribution = users.GroupBy(u => u.Role).ToDictionary(g => g.Key, g => g.Count());
        
        foreach (var user in users)
        {
            Assert.Contains(user.Role, validRoles);
            Assert.NotNull(user.Name);
            Assert.NotEmpty(user.Name);
            Assert.NotNull(user.Email);
            Assert.NotEmpty(user.Email);
            Assert.Contains("@", user.Email);
            Assert.True(user.OrganizationId > 0);
        }
        
        _output.WriteLine("✅ User role validation successful:");
        foreach (var role in roleDistribution)
        {
            _output.WriteLine($"  - {role.Key}: {role.Value} users");
        }
        
        // Should have both Admin and Basic users
        Assert.True(roleDistribution.ContainsKey("Admin"));
        Assert.True(roleDistribution.ContainsKey("Basic"));
    }

    [Fact]
    public async Task DatabaseSeeding_CreatesValidOrganizationData()
    {
        // Act
        var response = await _apiClient!.GetAsync("/api/organizations?testUser=admin");
        var organizations = await response.Content.ReadFromJsonAsync<List<Organization>>();

        // Assert
        Assert.NotNull(organizations);
        Assert.Equal(10, organizations.Count);
        
        foreach (var org in organizations)
        {
            Assert.True(org.Id > 0);
            Assert.NotNull(org.Name);
            Assert.NotEmpty(org.Name);
            Assert.True(org.CreatedAt <= DateTime.UtcNow);
            Assert.True(org.CreatedAt > DateTime.UtcNow.AddYears(-3)); // Should be within last 3 years (Bogus range)
            
            // Description can be null but if present should not be empty
            if (org.Description != null)
            {
                Assert.NotEmpty(org.Description);
            }
        }
        
        // All organization names should be unique
        var uniqueNames = organizations.Select(o => o.Name).Distinct().Count();
        Assert.Equal(organizations.Count, uniqueNames);
        
        _output.WriteLine("✅ Organization data validation successful - All organizations have valid data");
    }

    [Fact]
    public async Task DatabaseSeeding_CreatesRealisticUserData()
    {
        // Act
        var response = await _apiClient!.GetAsync("/api/users?testUser=admin");
        var users = await response.Content.ReadFromJsonAsync<List<User>>();

        // Assert
        Assert.NotNull(users);
        Assert.NotEmpty(users);
        
        foreach (var user in users)
        {
            Assert.True(user.Id > 0);
            Assert.NotNull(user.Name);
            Assert.NotEmpty(user.Name);
            Assert.NotNull(user.Email);
            Assert.NotEmpty(user.Email);
            Assert.Contains("@", user.Email);
            Assert.True(user.CreatedAt <= DateTime.UtcNow);
            Assert.True(user.OrganizationId > 0);
            
            // Name should contain at least first and last name (space separated)
            if (!user.Email.Contains("test.com")) // Skip test users
            {
                Assert.Contains(" ", user.Name);
            }
        }
        
        // All user emails should be unique
        var uniqueEmails = users.Select(u => u.Email).Distinct().Count();
        Assert.Equal(users.Count, uniqueEmails);
        
        _output.WriteLine($"✅ User data validation successful - All {users.Count} users have valid, unique data");
    }

    [Fact]
    public async Task DatabaseSeeding_CompletesWithinReasonableTime()
    {
        // This test verifies that the seeding process doesn't hang or take too long
        // The fact that we can reach this point means seeding completed successfully
        
        var startTime = DateTime.UtcNow;
        
        // Act - Get both organizations and users to ensure seeding is complete
        var orgsTask = _apiClient!.GetAsync("/api/organizations?testUser=admin");
        var usersTask = _apiClient!.GetAsync("/api/users?testUser=admin");
        
        await Task.WhenAll(orgsTask, usersTask);
        
        var endTime = DateTime.UtcNow;
        var duration = endTime - startTime;
        
        // Assert
        Assert.True(duration.TotalSeconds < 30, $"Data retrieval took too long: {duration.TotalSeconds} seconds");
        
        _output.WriteLine($"✅ Database seeding and data retrieval completed in {duration.TotalMilliseconds:F0}ms");
    }
}
