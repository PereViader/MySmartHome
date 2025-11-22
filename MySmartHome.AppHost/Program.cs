var builder = DistributedApplication.CreateBuilder(args);

var automations = builder.AddProject<Projects.MySmartHome_Automations>("automations")
    .WithExternalHttpEndpoints();

builder.AddProject<Projects.MySmartHome_Web>("web")
    .WithExternalHttpEndpoints();

builder.Build().Run();
