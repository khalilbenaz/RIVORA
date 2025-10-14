using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// API Service
var api = builder.AddProject<Projects.RVR_Framework_Api>("api");

builder.Build().Run();
