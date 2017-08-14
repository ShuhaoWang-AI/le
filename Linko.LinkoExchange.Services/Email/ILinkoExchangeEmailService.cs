using System.Collections.Generic;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.Email
{
    public interface ILinkoExchangeEmailService
    {
        void SendEmails(List<EmailEntry> emailEntries);
        List<EmailEntry> GetAllProgramEmailEntiresForUser(UserProfile userProfile, EmailType emailType, Dictionary<string, string> contentReplacements);
        List<EmailEntry> GetAllProgramEmailEntiresForUser(UserDto user, EmailType emailType, Dictionary<string, string> contentReplacements);
        EmailEntry GetEmailEntryForOrgRegProgramUser(OrganizationRegulatoryProgramUserDto user, EmailType emailType, Dictionary<string, string> contentReplacements);
        EmailEntry GetEmailEntryForUser(UserDto user, EmailType emailType, Dictionary<string, string> contentReplacements, OrganizationRegulatoryProgramDto orgRegProg);
        EmailEntry GetEmailEntryForUser(UserProfile user, EmailType emailType, Dictionary<string, string> contentReplacements, OrganizationRegulatoryProgram orgRegProg);
        EmailEntry GetEmailEntryForUser(UserDto user, EmailType emailType, Dictionary<string, string> contentReplacements, OrganizationRegulatoryProgram orgRegProg);
    }
}
