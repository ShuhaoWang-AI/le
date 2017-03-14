using Linko.LinkoExchange.Services.Dto;
using System;

namespace Linko.LinkoExchange.Services.Dto
{
    public class ReportPackageTemplateAssignmentDto
    {
        public int ReportPackageTemplateAssignmentId { get; set; }

        public int ReportPackageTemplateId { get; set; }
        public ReportPackageTemplateDto ReportPackageTemplate { get; set; }

        /// <summary>
        /// Typical value: Industry Regulatory Program id.
        /// </summary>
        public int OrganizationRegulatoryProgramId { get; set; }
        public OrganizationRegulatoryProgramDto OrganizationRegulatoryProgram { get; set; }
    }
}