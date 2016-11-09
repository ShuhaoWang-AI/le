namespace Linko.LinkoExchange.Services.Cache
{
    public sealed class CacheKey
    {
        private CacheKey() { }
        public static string SessionId = "SessionId";
        public static string OwinUserId = "OwinUserId";
        public static string OwinClaims = "OwinClaims";
        public static string UserProfileId = "UserProfileId";
        public static string FirstName = "UserName";
        public static string LastName = "LastName";
        public static string UserName = "UserName";
        public static string Email = "Email";
        public static string Token = "Token";
        public static string EmailRecipientRegulatoryProgramId = "EmailRecipientRegulatoryProgramId";
        public static string EmailRecipientOrganizationId = "EmailRecipientOrganizationId";
        public static string EmailRecipientRegulatoryOrganizationId = "EmailRecipientRegulatoryOrganizationId";
        public static string EmailSenderUserProfileId = "EmailSenderUserProfileId";
        public static string EmailRecipientUserProfileId = "EmailRecipientUserProfileId";
        
        public static string OrganizationRegulatoryProgramUserId = "OrganizationRegulatoryProgramUserId";
        public static string UserRole = "UserRole";
        public static string RegulatoryProgramName = "RegulatoryProgramName";
        public static string OrganizationRegulatoryProgramId = "OrganizationRegulatoryProgramId";
        public static string OrganizationName = "OrganizationName";
        public static string OrganizationId = "OrganizationId";
        public static string PortalName = "PortalName";
    }
}