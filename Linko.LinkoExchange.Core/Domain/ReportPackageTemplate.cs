using System;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    /// Represents a Report Package Template.
    /// </summary>
    public partial class ReportPackageTemplate
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int ReportPackageTemplateId { get; set; }

        /// <summary>
        /// Unique within a particular OrganizationRegulatoryProgramId and effective date.
        /// </summary>
        public string Name { get; set; }

        public string Description { get; set; }

        public DateTimeOffset EffectiveDateTimeUtc { get; set; }

        public DateTimeOffset? RetirementDateTimeUtc { get; set; }

        /// <summary>
        /// True: a signatory role is required to submit the Report Package. False, otherwise.
        /// </summary>
        public bool IsSubmissionBySignatoryRequired { get; set; }

        public int? CtsEventTypeId { get; set; }
        public virtual CtsEventType CtsEventType { get; set; }

        /// <summary>
        /// Typical value: Authority Regulatory Program id.
        /// </summary>
        public int OrganizationRegulatoryProgramId { get; set; }
        public virtual OrganizationRegulatoryProgram OrganizationRegulatoryProgram { get; set; }

        public bool IsActive { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }


        // Reverse navigation
        public virtual ICollection<ReportPackageTemplateAssignment> ReportPackageTemplateAssignments { get; set; }

        public virtual ICollection<ReportPackageTemplateElementCategory> ReportPackageTemplateElementCategories { get; set; }
    }
}
