using System;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    ///     Represents the different regulatory programs that an organization belongs to.
    /// </summary>
    public class OrganizationRegulatoryProgram
    {
        #region public properties

        /// <summary>
        ///     Primary key.
        /// </summary>
        public int OrganizationRegulatoryProgramId { get; set; }

        public int RegulatoryProgramId { get; set; }
        public virtual RegulatoryProgram RegulatoryProgram { get; set; }

        public int OrganizationId { get; set; }
        public virtual Organization Organization { get; set; }

        /// <summary>
        ///     NULL: an Authority OrganizationType since they are the ones who regulate other Organizations.
        /// </summary>
        public int? RegulatorOrganizationId { get; set; }

        public virtual Organization RegulatorOrganization { get; set; }

        /// <summary>
        ///     Industry specific column.
        /// </summary>
        public string AssignedTo { get; set; }

        /// <summary>
        ///     Authority: NPDES Permit Number.
        ///     Industry: Industry Number (in IPP).
        /// </summary>
        public string ReferenceNumber { get; set; }

        /// <summary>
        ///     True: the Organization has access to this particular Regulatory Program. False, otherwise.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        ///     True: the Organization has been removed from this particular Regulatory Program. False, otherwise.
        /// </summary>
        public bool IsRemoved { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }

        // Reverse navigation
        public virtual ICollection<OrganizationRegulatoryProgramModule> OrganizationRegulatoryProgramModules { get; set; }

        public virtual ICollection<OrganizationRegulatoryProgramSetting> OrganizationRegulatoryProgramSettings { get; set; }

        public virtual ICollection<OrganizationRegulatoryProgramUser> OrganizationRegulatoryProgramUsers { get; set; }

        public virtual ICollection<OrganizationRegulatoryProgramUser> InviterOrganizationRegulatoryProgramUsers { get; set; }

        public virtual ICollection<PermissionGroup> PermissionGroups { get; set; }

        public virtual ICollection<MonitoringPoint> MonitoringPoints { get; set; }

        public virtual ICollection<Parameter> Parameters { get; set; }

        public virtual ICollection<ParameterGroup> ParameterGroups { get; set; }

        public virtual ICollection<Sample> SampledBySamples { get; set; }

        public virtual ICollection<Sample> SampledForSamples { get; set; }

        public virtual ICollection<SampleRequirement> SampleRequirements { get; set; }

        public virtual ICollection<FileStore> FileStores { get; set; }

        public virtual ICollection<CtsEventType> CtsEventTypes { get; set; }

        public virtual ICollection<ReportElementType> ReportElementTypes { get; set; }

        public virtual ICollection<ReportPackageTemplate> ReportPackageTemplates { get; set; }

        public virtual ICollection<ReportPackageTemplateAssignment> ReportPackageTemplateAssignments { get; set; }

        public virtual ICollection<RepudiationReason> RepudiationReasons { get; set; }

        public virtual ICollection<ReportPackage> ReportPackages { get; set; }

        public virtual ICollection<DataSource> DataSources { get; set; }
        public virtual ICollection<ImportTempFile> ImportTempFiles { get; set; }
        #endregion
    }
}