using System;
using System.IO;
using Azure.Messaging.ServiceBus;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NLog.Extensions.Logging;
using SFA.DAS.Roatp.Functions.Configuration;
using SFA.DAS.Roatp.Functions.Infrastructure.ApiClients;
using SFA.DAS.Roatp.Functions.Infrastructure.BlobStorage;
using SFA.DAS.Roatp.Functions.Infrastructure.Databases;
using SFA.DAS.Roatp.Functions.Infrastructure.Tokens;
using SFA.DAS.Roatp.Functions.Services.Sectors;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureLogging((loggingBuilder) =>
    {
        loggingBuilder.AddNLog();
    })
    .ConfigureServices((services) =>
    {
        var configuration = BuildConfiguration(services);

        RegisterHttpClients(services, configuration);
        RegisterDatabaseContext(services, configuration);
        services.AddScoped<IDatamartBlobStorageFactory, DatamartBlobStorageFactory>();
        services.AddSingleton<ISectorProcessingService>((s) => new SectorProcessingService());
        services.AddSingleton<ServiceBusClient>(sp =>
        {
            return new ServiceBusClient(configuration["DASServiceBusConnectionString"]);
        });

    })
    .Build();

host.Run();

static IConfiguration BuildConfiguration(IServiceCollection services)
{
    var configBuilder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddEnvironmentVariables();

#if DEBUG
    configBuilder.AddJsonFile("local.settings.json", optional: false, reloadOnChange: true);
#else
    // Configure Azure Table Storage or other configuration providers here
#endif

    var config = configBuilder.Build();
    services.AddOptions();
    services.Configure<ConnectionStrings>(config.GetSection("ConnectionStrings"));
    services.Configure<QnaApiAuthentication>(config.GetSection("QnaApiAuthentication"));
    services.Configure<ApplyApiAuthentication>(config.GetSection("ApplyApiAuthentication"));
    services.Configure<GovUkApiAuthentication>(config.GetSection("GovUkApiAuthentication"));
    services.AddSingleton(config);
    return config;
}

static void RegisterHttpClients(IServiceCollection services, IConfiguration configuration)
{
    var acceptHeaderName = "Accept";
    var acceptHeaderValue = "application/json";
    var handlerLifeTime = TimeSpan.FromMinutes(5);

    services.AddHttpClient<IQnaApiClient, QnaApiClient>((serviceProvider, httpClient) =>
    {
        var qnaApiAuthentication = serviceProvider.GetService<IOptions<QnaApiAuthentication>>().Value;
        httpClient.BaseAddress = new Uri(qnaApiAuthentication.ApiBaseAddress);
        httpClient.DefaultRequestHeaders.Add(acceptHeaderName, acceptHeaderValue);

        var configuration = serviceProvider.GetService<IConfiguration>();
        if (!configuration["EnvironmentName"].Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase))
        {
            var generateTokenTask = BearerTokenGenerator.GenerateTokenAsync(qnaApiAuthentication.Identifier);
            httpClient.DefaultRequestHeaders.Authorization = generateTokenTask.GetAwaiter().GetResult();
        }
    })
    .SetHandlerLifetime(handlerLifeTime);

    services.AddHttpClient<IApplyApiClient, ApplyApiClient>((serviceProvider, httpClient) =>
    {
        var applyApiAuthentication = serviceProvider.GetService<IOptions<ApplyApiAuthentication>>().Value;
        httpClient.BaseAddress = new Uri(applyApiAuthentication.ApiBaseAddress);
        httpClient.DefaultRequestHeaders.Add(acceptHeaderName, acceptHeaderValue);

        var configuration = serviceProvider.GetService<IConfiguration>();
        if (!configuration["EnvironmentName"].Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase))
        {
            var generateTokenTask = BearerTokenGenerator.GenerateTokenAsync(applyApiAuthentication.Identifier);
            httpClient.DefaultRequestHeaders.Authorization = generateTokenTask.GetAwaiter().GetResult();
        }
    })
    .SetHandlerLifetime(handlerLifeTime);


    services.AddHttpClient<IGovUkApiClient, GovUkApiClient>((serviceProvider, httpClient) =>
    {
        var govUkApiAuthentication = serviceProvider.GetService<IOptions<GovUkApiAuthentication>>().Value;
        httpClient.BaseAddress = new Uri(govUkApiAuthentication.ApiBaseAddress);
        httpClient.DefaultRequestHeaders.Add(acceptHeaderName, acceptHeaderValue);

    })
        .SetHandlerLifetime(handlerLifeTime);
}

static void RegisterDatabaseContext(IServiceCollection services, IConfiguration configuration)
{
    services.AddDbContext<ApplyDataContext>((serviceProvider, options) =>
    {
        var connectionStrings = serviceProvider.GetService<IOptions<ConnectionStrings>>().Value;
        var applySqlConnectionString = connectionStrings.ApplySqlConnectionString;

        var connection = new SqlConnection(applySqlConnectionString);

        var configuration = serviceProvider.GetService<IConfiguration>();
        if (!configuration["EnvironmentName"].Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase))
        {
            var generateTokenTask = SqlTokenGenerator.GenerateTokenAsync();
            connection.AccessToken = generateTokenTask.GetAwaiter().GetResult();
        }

        options.UseSqlServer(connection,
        options => options.EnableRetryOnFailure());
    });
}
