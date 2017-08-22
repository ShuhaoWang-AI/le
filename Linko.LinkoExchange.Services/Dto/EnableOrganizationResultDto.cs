namespace Linko.LinkoExchange.Services.Dto
{
    public enum EnableOrganizationFailureReason
    {
        TooManyIndustriesForAuthority,
        TooManyUsersForThisIndustry
    }

    public class EnableOrganizationResultDto
    {
        #region public properties

        public bool IsSuccess { get; set; }
        public EnableOrganizationFailureReason FailureReason { get; set; }

        #endregion
    }
}