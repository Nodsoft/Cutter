var builder = DistributedApplication.CreateBuilder(new DistributedApplicationOptions()
{
    Args = args,
    DashboardApplicationName = "NSYS Cutter"
});

var githubClientId = builder.AddParameter("github-client-id");
var githubClientSecret = builder.AddParameter("github-client-secret", secret: true);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithRepl();

var database = postgres.AddDatabase("cutter-db", "cutter_dev");

var server = builder.AddProject<Projects.Nodsoft_Cutter_Server>("server")
    .WithReference(database, connectionName: "Database").WaitFor(database)
    .WithEnvironment("Auth__GitHub__ClientId", githubClientId)
    .WithEnvironment("Auth__GitHub__ClientSecret", githubClientSecret);

var serverMigrations = server.AddEFMigrations("server-db-migrations");

builder.Build().Run();
