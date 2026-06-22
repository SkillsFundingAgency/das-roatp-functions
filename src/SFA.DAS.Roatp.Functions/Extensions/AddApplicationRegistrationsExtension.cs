using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using SFA.DAS.Api.Common.Infrastructure;
using SFA.DAS.Roatp.Functions.Configuration;
using SFA.DAS.Roatp.Functions.Infrastructure.ApiClients;
using SFA.DAS.Roatp.Functions.Infrastructure.BlobStorage;
using SFA.DAS.Roatp.Functions.Infrastructure.Databases;
using SFA.DAS.Roatp.Functions.Infrastructure.Tokens;
using SFA.DAS.Roatp.Functions.Services.Sectors;

namespace SFA.DAS.Roatp.Functions.Extensions;

public static class AddApplicationRegistrationsExtension
{
    public static void AddApplicationRegistrations(this IServiceCollection services, IConfiguration configuration)
    {

        var qnaConfig = configuration.GetSection(nameof(QnaApiAuthentication)).Get<QnaApiAuthentication>();
        var applyConfig = configuration.GetSection(nameof(ApplyApiAuthentication)).Get<ApplyApiAuthentication>();
        var govUkConfig = configuration.GetSection(nameof(GovUkApiAuthentication)).Get<GovUkApiAuthentication>();
        IOuterApiClientConfiguration roatpOuterApiConfig = configuration.GetSection(nameof(RoatpOuterApiAuthentication)).Get<RoatpOuterApiAuthentication>();

        services.AddTransient<DefaultHeadersHandler>();

        RefitSettings refitSettings = new();
        services
            .AddRefitClient<IQnaApiClient>(refitSettings)
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(qnaConfig.ApiBaseAddress))
            .AddHttpMessageHandler<DefaultHeadersHandler>()
            .AddHttpMessageHandler(() => new InnerApiAuthenticationHeadersHandler(new AzureClientCredentialHelper(configuration), qnaConfig.Identifier))
            .SetHandlerLifetime(TimeSpan.FromMinutes(5));
        services
            .AddRefitClient<IApplyApiClient>(refitSettings)
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(applyConfig.ApiBaseAddress))
            .AddHttpMessageHandler<DefaultHeadersHandler>()
            .AddHttpMessageHandler(() => new InnerApiAuthenticationHeadersHandler(new AzureClientCredentialHelper(configuration), applyConfig.Identifier))
            .SetHandlerLifetime(TimeSpan.FromMinutes(5));
        services
            .AddRefitClient<IGovUkApiClient>(refitSettings)
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(govUkConfig.ApiBaseAddress))
            .AddHttpMessageHandler<DefaultHeadersHandler>()
            .SetHandlerLifetime(TimeSpan.FromMinutes(5));
        services
            .AddRefitClient<IRoatpOuterApiClient>(refitSettings)
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(roatpOuterApiConfig.BaseUrl))
            .AddHttpMessageHandler<DefaultHeadersHandler>()
            .AddHttpMessageHandler(() => new OuterApiAuthenticationHeadersHandlers(roatpOuterApiConfig))
            .SetHandlerLifetime(TimeSpan.FromMinutes(5));

        services.AddDbContext<ApplyDataContext>((serviceProvider, options) =>
        {
            var connectionStrings = configuration.GetSection(nameof(ConnectionStrings)).Get<ConnectionStrings>();
            var applySqlConnectionString = connectionStrings.ApplySqlConnectionString;
            var connection = new SqlConnection(applySqlConnectionString);

            if (!string.Equals(configuration["EnvironmentName"], "LOCAL", StringComparison.OrdinalIgnoreCase))
            {
                var token = SqlTokenGenerator.GenerateTokenAsync().GetAwaiter().GetResult();
                connection.AccessToken = token;
            }

            options.UseSqlServer(connection);
        });

        services.AddScoped<IDatamartBlobStorageFactory, DatamartBlobStorageFactory>();
        services.AddSingleton<ISectorProcessingService>(_ => new SectorProcessingService());

        services.AddSingleton(sp =>
        {
            var serviceBusConnectionString = configuration["DASServiceBusConnectionString"];
            return new ServiceBusAdministrationClient(serviceBusConnectionString);
        });

        services.AddSingleton<IServiceBusQueueInitializer, ServiceBusQueueInitializer>();
    }

    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var initializer = serviceProvider.GetRequiredService<IServiceBusQueueInitializer>();
        await initializer.InitializeAsync();
    }
}
