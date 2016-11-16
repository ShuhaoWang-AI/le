﻿using System;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    /// Represents an organization.
    /// </summary>
    public partial class Organization
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int OrganizationId { get; set; }

        public int OrganizationTypeId { get; set; }
        public virtual OrganizationType OrganizationType { get; set; }

        public string Name { get; set; }

        public string AddressLine1 { get; set; }

        public string AddressLine2 { get; set; }

        public string CityName { get; set; }

        public string ZipCode { get; set; }

        public int JurisdictionId { get; set; }
        public virtual Jurisdiction Jurisdiction { get; set; }

        public string PhoneNumber { get; set; }

        public int? PhoneExt { get; set; }

        public string FaxNumber { get; set; }

        public string WebsiteUrl { get; set; }

        /// <summary>
        /// Authority: NPDES Permit Number. 
        /// Industry: Permit Number.
        /// </summary>
        public string PermitNumber { get; set; }

        /// <summary>
        /// Authority specific column.
        /// </summary>
        public string Signer { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }


        // Reverse navigation
        public virtual System.Collections.Generic.ICollection<OrganizationRegulatoryProgram> OrganizationRegulatoryPrograms { get; set; }

        public virtual System.Collections.Generic.ICollection<OrganizationRegulatoryProgram> RegulatorOrganizationRegulatoryPrograms { get; set; }

        public virtual System.Collections.Generic.ICollection<OrganizationSetting> OrganizationSettings { get; set; }
    }
}