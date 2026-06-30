using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SFA.DAS.Roatp.Functions.Extensions;

FunctionsApplicationBuilder builder = FunctionsApplication.CreateBuilder(args);

builder.AddConfiguration();

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights()
    .AddApplicationRegistrations(builder.Configuration);

var app = builder.Build();

await AddApplicationRegistrationsExtension.InitializeAsync(app.Services);
await app.RunAsync();
