using Bogus;
using JJBussert.Aspire.Data;
using JJBussert.Aspire.Domain;
using Microsoft.EntityFrameworkCore;

namespace JJBussert.Aspire.DataService;

public class DataSeederWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DataSeederWorker> _logger;

    public DataSeederWorker(IServiceProvider serviceProvider, ILogger<DataSeederWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AspireDbContext>();

        try
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync(stoppingToken);
            _logger.LogInformation("Database ensured created");

            // Check if data already exists
            if (await context.Organizations.AnyAsync(stoppingToken))
            {
                _logger.LogInformation("Data already exists, skipping seeding");
                return;
            }

            _logger.LogInformation("Starting data seeding...");

            // Create organizations using Bogus
            var organizationFaker = new Faker<Organization>()
                .RuleFor(o => o.Name, f => f.Company.CompanyName())
                .RuleFor(o => o.Description, f => f.Company.CatchPhrase())
                .RuleFor(o => o.CreatedAt, f => f.Date.Past(2));

            var organizations = organizationFaker.Generate(10);
            await context.Organizations.AddRangeAsync(organizations, stoppingToken);
            await context.SaveChangesAsync(stoppingToken);

            _logger.LogInformation("Created {Count} organizations", organizations.Count);

            // Create users for each organization
            var userFaker = new Faker<User>()
                .RuleFor(u => u.Name, f => f.Name.FullName())
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.Role, f => f.PickRandom("Admin", "Basic"))
                .RuleFor(u => u.CreatedAt, f => f.Date.Past(1));

            var allUsers = new List<User>();

            foreach (var org in organizations)
            {
                var userCount = Random.Shared.Next(5, 21); // 5-20 users per org
                var users = userFaker.Generate(userCount);
                
                foreach (var user in users)
                {
                    user.OrganizationId = org.Id;
                }

                allUsers.AddRange(users);
            }

            // Ensure we have at least one test user for SWA CLI testing
            var testOrg = organizations.First();
            var testUser = new User
            {
                Name = "Test Admin User",
                Email = "admin@test.com",
                Role = "Admin",
                OrganizationId = testOrg.Id,
                CreatedAt = DateTime.UtcNow
            };
            allUsers.Add(testUser);

            var basicTestUser = new User
            {
                Name = "Test Basic User", 
                Email = "basic@test.com",
                Role = "Basic",
                OrganizationId = testOrg.Id,
                CreatedAt = DateTime.UtcNow
            };
            allUsers.Add(basicTestUser);

            await context.Users.AddRangeAsync(allUsers, stoppingToken);
            await context.SaveChangesAsync(stoppingToken);

            _logger.LogInformation("Created {Count} users across all organizations", allUsers.Count);
            _logger.LogInformation("Data seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during data seeding");
            throw;
        }
    }
}
