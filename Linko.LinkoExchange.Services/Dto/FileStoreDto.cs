﻿using System;

namespace Linko.LinkoExchange.Services.Dto
{
    public class FileStoreDto
    {
        #region public properties

        public int? FileStoreId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string OriginalFileName { get; set; }
        public double SizeByte { get; set; }
        public int FileTypeId { get; set; }
        public string FileType { get; set; }
        public int ReportElementTypeId { get; set; }
        public string ReportElementTypeName { get; set; }
        public int OrganizationRegulatoryProgramId { get; set; }
        public OrganizationRegulatoryProgramDto OrganizationRegulatoryProgram { get; set; }
        public DateTime UploadDateTimeLocal { get; set; }
        public int UploaderUserId { get; set; }
        public string UploaderUserFullName { get; set; }
        public DateTime? LastModificationDateTimeLocal { get; set; }
        public int? LastModifierUserId { get; set; }
        public string LastModifierUserFullName { get; set; }
        public byte[] Data { get; set; }
        public string MediaType { get; set; }
        public bool UsedByReports { get; set; }
        public int ReportPackageElementTypeId { get; internal set; } //only to be used when fetching files for possible inclusion in Report Package
        public bool IsAssociatedWithReportPackage { get; internal set; } // only to be used when displaying report package to show which samples are included
        public DateTime? LastSubmissionDateTimeLocal { get; internal set; }

        #endregion
    }
}