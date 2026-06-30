# Register of Apprenticeship Training Providers  - Functions

<img src="https://avatars.githubusercontent.com/u/9841374?s=200&v=4" align="right" alt="UK Government logo">

[![Build Status](https://sfa-gov-uk.visualstudio.com/Digital%20Apprenticeship%20Service/_apis/build/status%2FApprenticeships%20Providers%2Fdas-roatp-functions%20(New%20YAML%20Pipeline)?repoName=SkillsFundingAgency%2Fdas-roatp-functions&branchName=main)](https://sfa-gov-uk.visualstudio.com/Digital%20Apprenticeship%20Service/_build/latest?definitionId=3660&repoName=SkillsFundingAgency%2Fdas-roatp-functions&branchName=main)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=SkillsFundingAgency_das-roatp-functions&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=SkillsFundingAgency_das-roatp-functions)
[![License](https://img.shields.io/badge/license-MIT-lightgrey.svg?longCache=true&style=flat-square)](https://en.wikipedia.org/wiki/MIT_License)

## Developer Setup

### Pre-requisites

You will need following on your local:
* A clone of this repository
* Visual studio or similar IDE 
* .Net 10.0 SDK
* Azurite or similar local storage emulator for storing configuration and using blob storage
* SQL Database
* Azure Functions SDK
* Azure Service Bus instance hosted within Azure

### Dependencies

#### Internal dependencies
* [Qna Api](https://github.com/SkillsFundingAgency/das-qna-api)
* [Apply Internal Api](https://github.com/SkillsFundingAgency/das-apply-service/tree/master/src/SFA.DAS.ApplyService.InternalApi)
* [Roatp Outer Api](https://github.com/SkillsFundingAgency/das-apim-endpoints/tree/master/src/Roatp)

#### External dependencies
* [Gov UK](https://www.gov.uk/bank-holidays.json) for bank holidays

### Configuration

#### Configuring local storage
- Create a `Configuration `table in your (Development) local storage account.
- Obtain the [SFA.DAS.Roatp.Functions.json](https://github.com/SkillsFundingAgency/das-employer-config/blob/master/das-roatp-functions/SFA.DAS.Roatp.Functions.json) from the das-employer-config and adjust the SqlConnectionString property to match your local setup.
- Add a row to the Configuration table with fields: 
  - PartitionKey: LOCAL
  - RowKey: SFA.DAS.Roatp.Api_1.0
  - Data: {The contents of the `SFA.DAS.Roatp.Functions.json` file}

#### Configuring Functions project
- In the `SFA.DAS.Roatp.Functions` project, create a local.settings.json file with the following contents:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"",
    "ConfigNames": "SFA.DAS.Roatp.Functions",
    "EnvironmentName": "LOCAL",
    "ConfigurationStorageConnectionString": "UseDevelopmentStorage=true",
    "ApplicationExtractSchedule": "0 0 0 * * *",
    "GatewayExtractSchedule": "0 0 1 * * *",
    "AssessorExtractSchedule": "0 0 1 * * *",
    "FinanceExtractSchedule": "0 0 1 * * *",
    "AppealExtractSchedule": "0 0 1 * * *",
    "BankHolidayFulfillmentSchedule": "0 0 1 * * *",
    "DASServiceBusConnectionString": "Connection string pointing to an Azure Service Bus",
    "AzureWebJobs.AdminFileExtract.Disabled": "true",
    "AzureWebJobs.AppealExtract.Disabled": "true",
    "AzureWebJobs.AppealFileExtract.Disabled": "true",
    "AzureWebJobs.ApplicationExtract.Disabled": "true",
    "AzureWebJobs.ApplyFileExtract.Disabled": "true",
    "AzureWebJobs.AssessorExtract.Disabled": "true",
    "AzureWebJobs.BankHolidayFulfillment.Disabled": "true",
    "AzureWebJobs.FinanceExtract.Disabled": "true",
    "AzureWebJobs.GatewayExtract.Disabled": "true",
    "AzureWebJobs.UpdateProviderDetailsFunction.Disabled": "true"
  }
}
```

## About project 
The project has defined following functions:

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