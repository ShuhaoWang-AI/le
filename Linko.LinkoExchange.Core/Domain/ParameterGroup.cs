﻿using System;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    ///     Represents a static Parameter Group.
    /// </summary>
    public class ParameterGroup
    {
        #region public properties

        /// <summary>
        ///     Primary key.
        /// </summary>
        public int ParameterGroupId { get; set; }

        /// <summary>
        ///     Unique within a particular OrganizationRegulatoryProgramId.
        /// </summary>
        public string Name { get; set; }

        public string Description { get; set; }

        /// <summary>
        ///     Typical value: Authority Organization Regulatory Program id.
        /// </summary>
        public int OrganizationRegulatoryProgramId { get; set; }

        public virtual OrganizationRegulatoryProgram OrganizationRegulatoryProgram { get; set; }

        public bool IsActive { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }

        // Reverse navigation
        public virtual ICollection<ParameterGroupParameter> ParameterGroupParameters { get; set; }

        #endregion
    }
}