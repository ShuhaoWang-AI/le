namespace Linko.LinkoExchange.Web.ViewModels.User
{
    public class UserViewModel
    {
        public UserProfileViewModel UserProfile
        {
            get; set;
        }

        public UserKBQViewModel UserKBQ { get; set; }

        public UserSQViewModel UserSQ { get; set; }
    }
}