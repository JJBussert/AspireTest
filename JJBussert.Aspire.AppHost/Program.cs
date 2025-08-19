var builder = DistributedApplication.CreateBuilder(args);

// Add SQL Server with persistent lifetime for development
var sqlServer = builder.AddSqlServer("sql")
    .WithLifetime(ContainerLifetime.Persistent);

var database = sqlServer.AddDatabase("aspiredb");

// Add the API project
var api = builder.AddProject<Projects.JJBussert_Aspire_Api>("api")
    .WithReference(database)
    .WaitFor(database);

// Add the DataService worker
var dataService = builder.AddProject<Projects.JJBussert_Aspire_DataService>("dataservice")
    .WithReference(database)
    .WaitFor(database);

// Add the React frontend using NPM with SWA CLI for local auth emulation
var web = builder.AddNpmApp("web", "../JJBussert.Aspire.Web", "swa:start")
    .WithReference(api)
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .WithEnvironment("REACT_APP_API_URL", api.GetEndpoint("http"))
    .PublishAsDockerFile();

builder.Build().Run();
