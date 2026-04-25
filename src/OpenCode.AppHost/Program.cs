using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.OpenCode_DragonBall_Api>("dragonball-api")
       .WithReplicas(1);

builder.AddProject<Projects.OpenCode_Music_Api>("music-api")
       .WithReplicas(1);

builder.Build().Run();
