namespace SFA.DAS.Roatp.Functions.ApplyTypes
{
    public class OrganisationSectorExpertDeliveredTrainingTypes
    {
        public int Id { get; set; }
        public int OrganisationSectorExpertId { get; set; }
        public string DeliveredTrainingType { get; set; }
        public virtual OrganisationSectorExperts OrganisationSectorExperts { get; set; }
    }
}