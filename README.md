# ![crest](https://assets.publishing.service.gov.uk/government/assets/crests/org_crest_27px-916806dcf065e7273830577de490d5c7c42f36ddec83e907efe62086785f24fb.png) Digital Apprenticeships Service

##  Register of Apprenticeship Training Providers  - Functions

### Developer Setup

#### Requirements

- Install [.NET Core 3.1](https://www.microsoft.com/net/download)
- Install [Azure Functions SDK](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local)

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
    "ApplicationExtractSchedule": "0 0 */2 * * *"
  },

  "_comment": "USE THE BELOW SETTINGS SHOULD YOU WISH TO NOT USE AZURE TABLE STORAGE",
  "ConnectionStrings": {
    "ApplySqlConnectionString": "Data Source=.\\MSSQLLocalDB;Initial Catalog=SFA.DAS.ApplyService;Integrated Security=True"
  },

  "QnaApiAuthentication": {
    "Identifier": "https://tenant.onmicrosoft.com/das-at-api-as-ar",
    "ApiBaseAddress": "http://localhost:5554"
  }
}
```

### Application Extract

No specific configuration - run as Timer Trigger function. See `"ApplicationExtractSchedule"` for schedule