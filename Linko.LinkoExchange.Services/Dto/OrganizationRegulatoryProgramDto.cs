using System;

namespace Linko.LinkoExchange.Services.Dto
{
    public class OrganizationRegulatoryProgramDto
    {
        #region public properties

        public int OrganizationRegulatoryProgramId { get; set; }
        public int RegulatoryProgramId { get; set; }
        public ProgramDto RegulatoryProgramDto { get; set; }
        public int OrganizationId { get; set; }
        public int? RegulatorOrganizationId { get; set; }
        public OrganizationDto OrganizationDto { get; set; }
        public virtual OrganizationDto RegulatorOrganization { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsRemoved { get; set; }
        public string AssignedTo { get; set; }
        public string ReferenceNumber { get; set; }
        public bool HasSignatory { get; set; }
        public bool HasActiveAdmin { get; set; }

        //Localized most recent date (or null) found in tReportPackage for this OrganizationRegulatoryProgramId
        public DateTime? LastSubmissionDateTimeLocal { get; set; }

        #endregion
    }
}