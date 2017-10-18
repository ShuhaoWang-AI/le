namespace Linko.LinkoExchange.Services.Dto
{
    public class InvitationCheckEmailResultDto
    {
        public bool IsUserActiveInSameProgram { get; set; }
        public OrganizationRegulatoryProgramUserDto ExistingOrgRegProgramUser { get; set; }
    }
}