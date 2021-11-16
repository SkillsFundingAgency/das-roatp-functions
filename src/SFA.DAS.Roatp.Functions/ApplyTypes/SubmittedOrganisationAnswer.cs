using System;

namespace SFA.DAS.Roatp.Functions.ApplyTypes
{
    public class SubmittedOrganisationAnswer
    {
        public int Id { get; set; }
        public Guid ApplicationId { get; set; }
        public virtual ExtractedApplication ExtractedApplication { get; set; }

        public string Ukprn { get; set; }
        public string LegalName { get; set; }
        public string TradingName { get; set; }
        public string Website { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string AddressLine4 { get; set; }
        public string AddressLine5 { get; set; }
        public string Postcode { get; set; }
        public string CompanyNumber { get; set; }
        public string CharityNumber { get; set; }
        public string SoletraderOrPartnership { get; set; }
        public string OnRoatp { get; set; }
        public string ProviderRoute { get; set; }
        public string LevyPayingEmployer { get; set; }

        public string HasParentCompany { get; set; }
        public string ParentCompanyOrCharityNumber { get; set; }
        public string ParentCompanyOrCharityName { get; set; }
        public string CompaniesHouseDirectors { get; set; }
        public string CompaniesHousePSCs { get; set; }
        public string CharityComissionTrustees { get; set; }
        public string PeopleInControl { get; set; }

        public string OrganisationType { get; set; }
        public string EducationalInstituteType { get; set; }
        public string PublicBodyType { get; set; }
        public string SchoolType { get; set; }

        public string DescribeYourOrganisation { get; set; }
        public string ActivelyTradingFor { get; set; }

        public string RegisteredWithESFA { get; set; }
        public string ReceivingFundingFromESFA { get; set; }
        public string MonitoredByOfficeOfStudents { get; set; }
        public string FundedByOfficeOfStudents { get; set; }

        public string OfferInitialTeacherTraining { get; set; }
        public string PostgraduateTeachingApprenticeshipOnly { get; set; }
        public string HadFullOfstedInspection { get; set; }
        public string GotFullOfstedGrade { get; set; }
        public string FullOfstedGradeWithinLast3Years { get; set; }
        public string OverallEffectivenessOfstedGrade { get; set; }
        public string ApprenticeshipsOfstedGrade { get; set; }
        public string HadMonitoringVisit { get; set; }
        public string HadTwoInsufficientMonitoringVisits { get; set; }
        public string HadMonitoringVisitGradedInadequateInLast18Months { get; set; }
        public string MaintainedFundingSinceFullOfstedInspection { get; set; }
        public string HadShortOfstedInspectionWithinLast3Years { get; set; }
        public string MaintainedGradeInShortOfstedInspection { get; set; }
        public string DeliveredApprenticeshipTrainingAsSubcontractor { get; set; }
    }
}
