using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog.Extensions.Logging;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.Roatp.Functions.Configuration;
using SFA.DAS.Roatp.Functions.Infrastructure.ApiClients;
using SFA.DAS.Roatp.Functions.Infrastructure.Databases;
using SFA.DAS.Roatp.Functions.Infrastructure.Tokens;
using SFA.DAS.Roatp.Functions.NLog;
using System;
using System.IO;

[assembly: FunctionsStartup(typeof(SFA.DAS.Roatp.Functions.Startup))]

namespace SFA.DAS.Roatp.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            AddNLog(builder);

            var serviceProvider = builder.Services.BuildServiceProvider();
            BuildConfigurationSettings(builder, serviceProvider);

            BuildHttpClients(builder);
            BuildDataContext(builder);
        }

        private static void AddNLog(IFunctionsHostBuilder builder)
        {
            var nLogConfiguration = new NLogConfiguration();

            builder.Services.AddLogging((options) =>
            {
                options.AddFilter(typeof(Startup).Namespace, LogLevel.Information);
                options.SetMinimumLevel(LogLevel.Trace);
                options.AddNLog(new NLogProviderOptions
                {
                    CaptureMessageTemplates = true,
                    CaptureMessageProperties = true
                });
                options.AddConsole();

                nLogConfiguration.ConfigureNLog();
            });
        }

        private static void BuildConfigurationSettings(IFunctionsHostBuilder builder, ServiceProvider serviceProvider)
        {
            var configuration = serviceProvider.GetService<IConfiguration>();

            var configBuilder = new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables();

#if DEBUG
            configBuilder.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
#else
            configBuilder.AddAzureTableStorage(options =>
            {
                options.ConfigurationKeys = new[] { "SFA.DAS.RoatpFunctions"};
                options.StorageConnectionString = configuration["ConfigurationStorageConnectionString"];
                options.EnvironmentName = configuration["EnvironmentName"];
                options.PreFixConfigurationKeys = false;
            });
#endif

            var config = configBuilder.Build();
            builder.Services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), config));

            builder.Services.AddOptions();
            builder.Services.Configure<ConnectionStrings>(config.GetSection("ConnectionStrings"));
            builder.Services.Configure<QnaApiAuthentication>(config.GetSection("QnaApiAuthentication"));
        }

        private static void BuildHttpClients(IFunctionsHostBuilder builder)
        {
            var acceptHeaderName = "Accept";
            var acceptHeaderValue = "application/json";
            var handlerLifeTime = TimeSpan.FromMinutes(5);

            builder.Services.AddHttpClient<IQnaApiClient, QnaApiClient>((serviceProvider, httpClient) =>
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
        }

        private static void BuildDataContext(IFunctionsHostBuilder builder)
        {
            builder.Services.AddDbContext<ApplyDataContext>((serviceProvider, options) =>
            {
                var connectionStrings = serviceProvider.GetService<IOptions<ConnectionStrings>>().Value;
                var applySqlConnectionString = connectionStrings.ApplySqlConnectionString;

                var connection = new SqlConnection(applySqlConnectionString);

                var configuration = serviceProvider.GetService<IConfiguration>(); ;
                if (!configuration["EnvironmentName"].Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase))
                {
                    var generateTokenTask = SqlTokenGenerator.GenerateTokenAsync();
                    connection.AccessToken = generateTokenTask.GetAwaiter().GetResult();
                }

                options.UseSqlServer(connection);
            });
        }
    }
}
