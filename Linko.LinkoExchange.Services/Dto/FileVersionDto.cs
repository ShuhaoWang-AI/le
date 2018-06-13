using System;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Services.Dto
{
    public class FileVersionDto
    {
        #region public properties

        public int? FileVersionId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        /// <summary>
        /// Authority OrganizationRegulatoryProgramId
        /// </summary>
        public int OrganizationRegulatoryProgramId { get; set; }
        public OrganizationRegulatoryProgramDto OrganizationRegulatoryProgram { get; set; }
        public DateTime LastModificationDateTimeLocal { get; internal set; }
        public int? LastModifierUserId { get; internal set; }
        public string LastModifierFullName { get; internal set; }
        public List<FileVersionFieldDto> FileVersionFields { get; set; }

        #endregion
    }
}