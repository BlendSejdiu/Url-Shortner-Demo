var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume().AddDatabase("url-shortneer");

var redis = builder.AddRedis("redis");

builder.AddProject<Projects.UrlShortner>("urlshortener").WithReference(postgres).WithReference(redis).WaitFor(postgres).WaitFor(redis);

builder.Build().Run();
