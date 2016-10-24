namespace Linko.LinkoExchange.Services.Dto
{
    public class UserDto
    {
        public int OrgRegProgUserId { get; set; }
        public int UserProfileId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public int AuthorityId { get; set; }
        public int IndustryId { get; set; }
        public string IndustryName { get; set; }
        public string Password { get; set; }
        public string PasswordHash { get; set; }
        public bool IsEnabled { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }


    }
}
