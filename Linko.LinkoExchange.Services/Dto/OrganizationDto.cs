namespace Linko.LinkoExchange.Services.Dto
{
    public class OrganizationDto
    {
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string CityName { get; set; }
        public string State { get; set; }
        public int OrganizationId { get; set; }
        public int OrganizationTypeId { get; set; }
        public OrganizationTypeDto OrganizationType { get; set; }
        public string OrganizationName { get; set; }
        public string ZipCode { get; set; }
        public string PhoneNumber { get; set; }
        public string PhoneExt { get; set; }
        public string FaxNumber { get; set; }
        public string WebsiteURL { get; set; }
        public string PermitNumber { get; set; }
        public string Signer { get; set; }
        public string Classification { get; set; }
    }

    public class AuthorityDto : OrganizationDto
    {
        public string EmailContactInfoName { get; set; }
        public string EmailContactInfoPhone { get; set; }
        public string EmailContactInfoEmailAddress { get; set; }
        public int RegulatoryProgramId { get; set; }
        public int OrganizationRegulatoryProgramId { get; set; }
    }
}
