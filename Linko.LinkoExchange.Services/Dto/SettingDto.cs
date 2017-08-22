using Linko.LinkoExchange.Core.Enum;

namespace Linko.LinkoExchange.Services.Dto
{
    public class SettingDto
    {
        #region public properties

        public SettingType TemplateName { get; set; }
        public OrganizationTypeName OrgTypeName { get; set; }
        public string Value { get; set; }
        public string DefaultValue { get; set; }
        public string Description { get; set; }

        #endregion
    }
}