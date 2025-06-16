using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Roatp.Functions.Configuration;
using SFA.DAS.Roatp.Functions.Infrastructure.ApiClients;
using SFA.DAS.Roatp.Functions.Infrastructure.BlobStorage;
using SFA.DAS.Roatp.Functions.Infrastructure.Databases;
using SFA.DAS.Roatp.Functions.Infrastructure.Tokens;
using SFA.DAS.Roatp.Functions.Services.Sectors;

[assembly: FunctionsStartup(typeof(SFA.DAS.Roatp.Functions.Startup))]

namespace SFA.DAS.Roatp.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var serviceProvider = builder.Services.BuildServiceProvider();
            BuildConfigurationSettings(builder, serviceProvider);

            BuildHttpClients(builder);
            BuildDataContext(builder);
            BuildDependencyInjection(builder);
            BuildServiceBusQueues(builder).GetAwaiter().GetResult();
        }

        private static async Task BuildServiceBusQueues(IFunctionsHostBuilder builder)
        {
            TimeSpan lockDuration = TimeSpan.FromMinutes(5);
            const int maxDeliveryCount = 10;
            const int maxSizeInMB = 5120;

            var configuration = builder.Services.BuildServiceProvider().GetService<IConfiguration>();

            var serviceBusConnectionString = configuration["DASServiceBusConnectionString"];

            var managementClient = new ManagementClient(serviceBusConnectionString);

            var applyFileExtractQueue = configuration["ApplyFileExtractQueue"];
            if (!await managementClient.QueueExistsAsync(applyFileExtractQueue))
            {
                var applyQueueDescription = new QueueDescription(applyFileExtractQueue)
                {
                    LockDuration = lockDuration,
                    MaxDeliveryCount = maxDeliveryCount,
                    MaxSizeInMB = maxSizeInMB
                };
                await managementClient.CreateQueueAsync(applyQueueDescription);
            }

            var adminFileExtractQueue = configuration["AdminFileExtractQueue"];
            if (!await managementClient.QueueExistsAsync(adminFileExtractQueue))
            {
                var adminQueueDescription = new QueueDescription(adminFileExtractQueue)
                {
                    LockDuration = lockDuration,
                    MaxDeliveryCount = maxDeliveryCount,
                    MaxSizeInMB = maxSizeInMB
                };
                await managementClient.CreateQueueAsync(adminQueueDescription);
            }

            var appealFileExtractQueue = configuration["AppealFileExtractQueue"];
            if (!await managementClient.QueueExistsAsync(appealFileExtractQueue))
            {
                var appealQueueDescription = new QueueDescription(appealFileExtractQueue)
                {
                    LockDuration = lockDuration,
                    MaxDeliveryCount = maxDeliveryCount,
                    MaxSizeInMB = maxSizeInMB
                };
                await managementClient.CreateQueueAsync(appealQueueDescription);
            }
        }

        private static void BuildConfigurationSettings(IFunctionsHostBuilder builder, ServiceProvider serviceProvider)
        {
            var configuration = serviceProvider.GetService<IConfiguration>();

            var configBuilder = new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables();

#if DEBUG
            configBuilder.AddJsonFile("local.settings.json", optional: false, reloadOnChange: true);
#else
            configBuilder.AddAzureTableStorage(options =>
            {
                options.ConfigurationKeys = new[] { "SFA.DAS.Roatp.Functions"};
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
            builder.Services.Configure<ApplyApiAuthentication>(config.GetSection("ApplyApiAuthentication"));
            builder.Services.Configure<GovUkApiAuthentication>(config.GetSection("GovUkApiAuthentication"));
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

            builder.Services.AddHttpClient<IApplyApiClient, ApplyApiClient>((serviceProvider, httpClient) =>
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


            builder.Services.AddHttpClient<IGovUkApiClient, GovUkApiClient>((serviceProvider, httpClient) =>
                {
                    var govUkApiAuthentication = serviceProvider.GetService<IOptions<GovUkApiAuthentication>>().Value;
                    httpClient.BaseAddress = new Uri(govUkApiAuthentication.ApiBaseAddress);
                    httpClient.DefaultRequestHeaders.Add(acceptHeaderName, acceptHeaderValue);

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

                var configuration = serviceProvider.GetService<IConfiguration>();
                if (!configuration["EnvironmentName"].Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase))
                {
                    var generateTokenTask = SqlTokenGenerator.GenerateTokenAsync();
                    connection.AccessToken = generateTokenTask.GetAwaiter().GetResult();
                }

                options.UseSqlServer(connection);
            });
        }

        private static void BuildDependencyInjection(IFunctionsHostBuilder builder)
        {
            builder.Services.AddScoped<IDatamartBlobStorageFactory, DatamartBlobStorageFactory>();
            builder.Services.AddSingleton<ISectorProcessingService>((s) =>
            {
                return new SectorProcessingService();
            });
        }
    }
}
