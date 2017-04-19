﻿using System;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Services.Dto
{
    //TODO: to add more, 
    // Here is just for Cor usage
    public class ReportPackageDto
    {
        public int ReportPackageId { get; set; }
        public string Name { get; set; }
        public DateTime SubMissionDateTimeLocal { get; set; }
        public int OrganizationRegulatoryProgramId { get; set; }
        public OrganizationRegulatoryProgramDto OrganizationRegulatoryProgramDto { get; set; }
        public string RecipientOrganizationName { get; internal set; }
        public string RecipientOrganizationAddressLine1 { get; internal set; }
        public string RecipientOrganizationAddressLine2 { get; internal set; }
        public string RecipientOrganizationCityName { get; internal set; }
        public string RecipientOrganizationJurisdictionName { get; internal set; }
        public string RecipientOrganizationZipCode { get; internal set; }
        public DateTime PeriodStartDateTimeLocal { get; internal set; }
        public DateTime PeriodEndDateTimeLocal { get; internal set; }
        public bool IsSubmissionBySignatoryRequired { get; internal set; }
        public int ReportStatusId { get; internal set; }
        public string OrganizationName { get; internal set; }
        public string OrganizationAddressLine1 { get; internal set; }
        public string OrganizationAddressLine2 { get; internal set; }
        public string OrganizationCityName { get; internal set; }
        public string OrganizationJurisdictionName { get; internal set; }
        public DateTime CreationDateTimeLocal { get; internal set; }
        public List<FileStoreDto> AttachmentDtos { get; set; }
        public List<SampleDto> SamplesDtos { get; set; }
        public List<ReportPackageELementTypeDto> CertificationDtos { get; set; }
    }
}