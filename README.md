# ![crest](https://assets.publishing.service.gov.uk/government/assets/crests/org_crest_27px-916806dcf065e7273830577de490d5c7c42f36ddec83e907efe62086785f24fb.png) Digital Apprenticeships Service

##  Register of Apprenticeship Training Providers  - Functions

### Developer Setup

#### Requirements

- Install [.NET Core 3.1](https://www.microsoft.com/net/download)
- Install [Azure Functions SDK](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local)

### Configuration

1) Create a local.settings.json file (Copy to Output Directory = Copy always) with the following contents:

```json
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "AzureWebJobsDashboard": "UseDevelopmentStorage=true",
		"FUNCTIONS_WORKER_RUNTIME": "dotnet",

        "AppName": "das-roatp-functions",
		"ConfigNames": "SFA.DAS.RoatpFunctions",
        "EnvironmentName": "LOCAL",
        "ConfigurationStorageConnectionString": "UseDevelopmentStorage=true",
		"LoggingRedisConnectionString": "localhost"
    }
}
```

### Application Report

No specific configuration - run as Timer Trigger function