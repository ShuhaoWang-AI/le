namespace Linko.LinkoExchange.Services.Dto
{
    public class UserDto
    { 
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public int AuthorityId { get; set; }
        public int IndustryId { get; set; }
        public string IndustryName { get; set; }
        public string Password { get; set; }
        public string PasswordHash { get; set; }
    }
}
