using Linko.LinkoExchange.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Linko.LinkoExchange.Web.ViewModels.User
{
    public class RegistrationViewModel
    {
        public string Token { get; set; }
        public UserProfileViewModel UserProfile { get; set; }
        public UserKBQViewModel UserKBQ { get; set; }
        public UserSQViewModel UserSQ { get; set; }
    }
}