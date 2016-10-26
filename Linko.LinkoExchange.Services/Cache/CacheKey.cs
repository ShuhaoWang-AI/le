namespace Linko.LinkoExchange.Services.Cache
{
    public sealed class CacheKey
    {
        private CacheKey() { }
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
        internal static string UserRole;
        internal static string RegulatoryProgramName;
        internal static string OrganizationRegulatoryProgramId;
        internal static string OrganizationName;
        internal static string OrganizationId;
        internal static string PortalName;
    }
}