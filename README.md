## ⛔Never push sensitive information such as client id's, secrets or keys into repositories including in the README file⛔

# _Roatp Functions_

<img src="https://avatars.githubusercontent.com/u/9841374?s=200&v=4" align="right" alt="UK Government logo">

[![Build Status](https://sfa-gov-uk.visualstudio.com/Digital%20Apprenticeship%20Service/_apis/build/status%2FApprenticeships%20Providers%2Fdas-roatp-functions?repoName=SkillsFundingAgency%2Fdas-roatp-functions&branchName=refs%2Fpull%2F53%2Fmerge)](https://sfa-gov-uk.visualstudio.com/Digital%20Apprenticeship%20Service/_build/latest?definitionId=2374&repoName=SkillsFundingAgency%2Fdas-roatp-functions&branchName=refs%2Fpull%2F53%2Fmerge)

[![Quality Gate Status](
https://sonarcloud.io/api/project_badges/measure?project=SkillsFundingAgency_das-roatp-functions&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=SkillsFundingAgency_das-roatp-dunctions)

[![License](https://img.shields.io/badge/license-MIT-lightgrey.svg?longCache=true&style=flat-square)](https://en.wikipedia.org/wiki/MIT_License)


## 🚀 Installation

### Pre-Requisites
* A clone of this repository
* Clone and run the database publish for das-apply-service: https://github.com/SkillsFundingAgency/das-apply-service


### Dependencies
There are no dependencies, but you will have to have the applyService database running locally (see note about das-apply-service in pre-requisites)

- Install [.NET 8](https://www.microsoft.com/net/download)
- Install [Azure Functions SDK](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local)
- Azure Service Bus instance hosted within Azure

### Configuration

1) Create a local.settings.json file (Copy to Output Directory = Copy if newer) with the following contents:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "AppName": "das-roatp-functions",
    "EnvironmentName": "LOCAL",
    "ConfigurationStorageConnectionString": "UseDevelopmentStorage=true",
    "LoggingRedisConnectionString": "localhost",
    "ApplicationExtractSchedule": "0 0 0 * * *",
    "GatewayExtractSchedule": "0 0 1 * * *",
    "AssessorExtractSchedule": "0 0 1 * * *",
    "FinanceExtractSchedule": "0 0 1 * * *",
    "AppealExtractSchedule": "0 0 1 * * *",
	"BankHolidayFulfillmentSchedule": "0 0 1 * * *",
	"ApplyFileExtractQueue": "SFA.DAS.Roatp.Functions.ApplyFileExtract",
	"AdminFileExtractQueue": "SFA.DAS.Roatp.Functions.AdminFileExtract",
	"AppealFileExtractQueue": "SFA.DAS.Roatp.Functions.AppealFileExtract",
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
  "ApplyApiAuthentication": {
    "Identifier": "https://tenant.onmicrosoft.com/das-at-api-as-ar",
    "ApiBaseAddress": "https://localhost:6000"
  },
  "GovUkApiAuthentication": {
    "ApiBaseAddress": "https://www.gov.uk"
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

### Appeal Extract

No specific configuration - run as Timer Trigger function. See `"AppealExtractSchedule"` for schedule.

Note also fires off a Service Bus message to Appeal File Extract for any file uploads.

### Appeal File Extract

No specific configuration - runs as Service Bus trigger function. See `"DASServiceBusConnectionString"` and `"AppealFileExtractQueue"` for Service Bus information.

### Bank Holiday Fulfillment

No specific configuration - run as Timer Trigger function. See `"BankHolidayFulfillmentSchedule"` for schedule.