namespace Linko.LinkoExchange.Services.Dto
{
    public class OrganizationDto
    {
        public string AddressLine1 { get; set; }
        public object AddressLine2 { get; set; }
        public object CityName { get; set; }
        public int OrganizationId { get; set; }
        public int OrganizationTypeId { get; set; }
        public OrganizationTypeDto OrganizationType { get; set; }
        public string OrganizationName { get; set; }
        public object ZipCode { get; set; }
        public string PhoneNumber { get; set; }
        public string PhoneExt { get; set; }
        public string FaxNumber { get; set; }
        public string WebsiteURL { get; set; } 
        //TODO others...
    }
}
