using JJBussert.Aspire.Data;
using JJBussert.Aspire.DataService;

var builder = Host.CreateApplicationBuilder(args);

// Add Aspire service defaults
builder.AddServiceDefaults();

// Add Entity Framework with Aspire SQL Server integration
builder.AddSqlServerDbContext<AspireDbContext>("aspiredb");

// Add the worker service
builder.Services.AddHostedService<DataSeederWorker>();

var host = builder.Build();
host.Run();
