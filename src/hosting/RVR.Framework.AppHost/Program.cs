using RVR.Framework.Aspire;

var builder = DistributedApplication.CreateBuilder(args);

// Infrastructure
var sql = builder.AddRivoraSqlServer();
var db = sql.AddDatabase("RVRFrameworkDb");
var cache = builder.AddRivoraRedis();
var messaging = builder.AddRivoraRabbitMQ();

// RIVORA API
var api = builder.AddProject<Projects.RVR_Framework_Api>("rivora-api")
    .WithExternalHttpEndpoints()
    .WithReference(db)
    .WithReference(cache)
    .WithReference(messaging)
    .WaitFor(db)
    .WaitFor(cache)
    .WaitFor(messaging);

// RIVORA Admin
var admin = builder.AddProject<Projects.RVR_Framework_Admin>("rivora-admin")
    .WithExternalHttpEndpoints()
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
