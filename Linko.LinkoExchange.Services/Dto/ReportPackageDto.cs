using System;

namespace Linko.LinkoExchange.Services.Dto
{

    //TODO: to add more, 
    // Here is just for Cor usage
    public class ReportPackageDto
    {
        public int ReportPackageId { get; set; }
        public string Name { get; set; }
        public DateTime SubMissionDateTime { get; set; }
        public DateTime SubMissionDateTimeLocal { get; set; }
        public int OrganizationRegulatoryProgramId { get; set; }
        public OrganizationRegulatoryProgramDto OrganizationRegulatoryProgramDto { get; set; }
    }
}