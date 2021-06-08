# ![crest](https://assets.publishing.service.gov.uk/government/assets/crests/org_crest_27px-916806dcf065e7273830577de490d5c7c42f36ddec83e907efe62086785f24fb.png) Digital Apprenticeships Service

##  Register of Apprenticeship Training Providers  - Functions

### Developer Setup

#### Requirements

- Install [.NET Core 3.1](https://www.microsoft.com/net/download)
- Install [Azure Functions SDK](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local)
- Azure Service Bus instance hosted within Azure

### Configuration

1) Create a local.settings.json file (Copy to Output Directory = Copy if newer) with the following contents:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",

    "AppName": "das-roatp-functions",
    "EnvironmentName": "LOCAL",
    "ConfigurationStorageConnectionString": "UseDevelopmentStorage=true",
    "LoggingRedisConnectionString": "localhost",
    "ApplicationExtractSchedule": "0 0 0 * * *",
    "GatewayExtractSchedule": "0 0 1 * * *",
    "AssessorExtractSchedule": "0 0 1 * * *",
    "FinanceExtractSchedule": "0 0 1 * * *",
	"ApplyFileExtractQueue": "SFA.DAS.Roatp.Functions.ApplyFileExtract",
	"AdminFileExtractQueue": "SFA.DAS.Roatp.Functions.AdminFileExtract",
	"DASServiceBusConnectionString": "Connection string pointing to an Azure Service Bus"
  },
  "ConnectionStrings": {
    "ApplySqlConnectionString": "Data Source=.\\MSSQLLocalDB;Initial Catalog=SFA.DAS.ApplyService;Integrated Security=True",
    "DatamartBlobStorageConnectionString": "UseDevelopmentStorage=true"
  },
  "QnaApiAuthentication": {
    "Identifier": "https://tenant.onmicrosoft.com/das-at-api-as-ar",
    "ApiBaseAddress": "http://localhost:5554"
  },
  "RoatpApplyApiAuthentication": {
    "Identifier": "https://tenant.onmicrosoft.com/das-at-api-as-ar",
    "ApiBaseAddress": "https://localhost:6000"
  }
}
```

### Application Extract

No specific configuration - run as Timer Trigger function. See `"ApplicationExtractSchedule"` for schedule.

Note also fires off a Service Bus message to Apply File Extract for any file uploads.

### Apply File Extract

No specific configuration - runs as Service Bus trigger function. See `"DASServiceBusConnectionString"` and `"ApplyFileExtractQueue"` for Service Bus information.

### Gateway Extract

No specific configuration - run as Timer Trigger function. See `"GatewayExtractSchedule"` for schedule.

Note also fires off a Service Bus message to Admin File Extract for any file uploads.

### Assessor Extract

No specific configuration - run as Timer Trigger function. See `"AssessorExtractSchedule"` for schedule.

Note also fires off a Service Bus message to Admin File Extract for any file uploads.

### Finance Extract

No specific configuration - run as Timer Trigger function. See `"FinanceExtractSchedule"` for schedule.

Note also fires off a Service Bus message to Admin File Extract for any file uploads.

### Admin File Extract

No specific configuration - runs as Service Bus trigger function. See `"DASServiceBusConnectionString"` and `"AdminFileExtractQueue"` for Service Bus information.