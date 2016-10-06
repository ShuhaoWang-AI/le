using Linko.LinkoExchange.Services.Common;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Services
{
    public interface IOrganizationService
    {
        IEnumerable<OrganizationSettings> GetOrganizationSettingsByUserId(string userId);
        OrganizationSettings GetOrganizationSettingsById(string organizationid);
    }
}
