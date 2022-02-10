namespace SFA.DAS.Roatp.Functions.ApplyTypes
{
    public class OrganisationSectorExpertDeliveredTrainingType
    {
        public int Id { get; set; }
        public int OrganisationSectorExpertId { get; set; }
        public string DeliveredTrainingType { get; set; }
        public virtual OrganisationSectorExpert OrganisationSectorExpert { get; set; }
    }
}