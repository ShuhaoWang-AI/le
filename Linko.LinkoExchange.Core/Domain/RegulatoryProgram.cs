﻿using System;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    ///     Represents a regulatory program.
    /// </summary>
    public class RegulatoryProgram
    {
        #region public properties

        /// <summary>
        ///     Primary key.
        /// </summary>
        public int RegulatoryProgramId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }

        // Reverse navigation
        public virtual ICollection<OrganizationRegulatoryProgram> OrganizationRegulatoryPrograms { get; set; }

        public virtual ICollection<OrganizationTypeRegulatoryProgram> OrganizationTypeRegulatoryPrograms { get; set; }

        #endregion
    }
}