namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    ///     Represents an assignment of a Report Package Template.
    ///     Typically, an Authority assigns certain Report Package Templates to Industries.
    /// </summary>
    public class ReportPackageTemplateAssignment
    {
        #region public properties

        /// <summary>
        ///     Primary key.
        /// </summary>
        public int ReportPackageTemplateAssignmentId { get; set; }

        public int ReportPackageTemplateId { get; set; }
        public virtual ReportPackageTemplate ReportPackageTemplate { get; set; }

        /// <summary>
        ///     Typical value: Industry Regulatory Program id.
        /// </summary>
        public int OrganizationRegulatoryProgramId { get; set; }

        public virtual OrganizationRegulatoryProgram OrganizationRegulatoryProgram { get; set; }

        #endregion
    }
}