namespace Linko.LinkoExchange.Services.Dto
{
    public class OrganizationDto
    {
        public string AddressLine1 { get; set; }
        public object AddressLine2 { get; set; }
        public object City { get; set; }
        public int OrganizationId { get; set; }
        public int OrganizationTypeId { get; set; }
        public OrganizationTypeDto OrganizationType { get; set; }
        public string OrganizationName { get; set; }
        public object ZipCode { get; set; }
        //TODO others...
    }
}
