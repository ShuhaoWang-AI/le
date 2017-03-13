using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Mapping;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Linko.LinkoExchange.Services.Dto;
using System.Data.Entity;

namespace Linko.LinkoExchange.Services.Report
{
    public class ReportElementService : IReportElementService
    {
        private readonly LinkoExchangeContext _dbContext;
        private readonly IMapHelper _mapHelper;
        private readonly ILogger _logger;

        private int _orgRegProgramId;

        public ReportElementService(LinkoExchangeContext dbContext,
            IHttpContextService httpContext,
            IMapHelper mapHelper,
            ILogger logger)
        {
            _dbContext = dbContext;
            _mapHelper = mapHelper;
            _orgRegProgramId = int.Parse(httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            _logger = logger;
        }

        public IEnumerable<CertificationTypeDto> GetCertificationTypes()
        {
            var certTypes = new List<CertificationTypeDto>();
            var certReportElementCategoryId = _dbContext.ReportElementCategories
                .Single(r => r.Name == ReportElementCategoryName.Certifications.ToString()).ReportElementCategoryId;

            var foundReportElementTypes = _dbContext.ReportElementTypes
                .Include(c => c.CtsEventType)
                .Where(c => c.OrganizationRegulatoryProgramId == _orgRegProgramId
                    && c.ReportElementCategoryId == certReportElementCategoryId)
                .ToList();
            foreach (var reportElementType in foundReportElementTypes)
            {
                var dto = _mapHelper.GetCertificationTypeDtoFromReportElementType(reportElementType);
                certTypes.Add(dto);
            }
            return certTypes;
        }

        public void SaveCertificationType(CertificationTypeDto certType)
        {
            ReportElementType certificationTypeToPersist = null;
            if (certType.CertificationTypeID.HasValue && certType.CertificationTypeID.Value > 0)
            {
                //Update existing
                certificationTypeToPersist = _dbContext.ReportElementTypes.Single(c => c.ReportElementTypeId == certType.CertificationTypeID);
                certificationTypeToPersist = _mapHelper.GetReportElementTypeFromCertificationTypeDto(certType, certificationTypeToPersist);
            }
            else
            {
                //Get new
                certificationTypeToPersist = _mapHelper.GetReportElementTypeFromCertificationTypeDto(certType);
                _dbContext.ReportElementTypes.Add(certificationTypeToPersist);
            }
            _dbContext.SaveChanges();

        }

        public void SaveAttachmentType(AttachmentTypeDto attachmentType)
        {
            ReportElementType attachmentTypeToPersist = null;
            if (attachmentType.AttachmentTypeID.HasValue && attachmentType.AttachmentTypeID.Value > 0)
            {
                //Update existing
                attachmentTypeToPersist = _dbContext.ReportElementTypes.Single(c => c.ReportElementTypeId == attachmentType.AttachmentTypeID);
                attachmentTypeToPersist = _mapHelper.GetReportElementTypeFromAttachmentTypeDto(attachmentType, attachmentTypeToPersist);
            }
            else
            {
                //Get new
                attachmentTypeToPersist = _mapHelper.GetReportElementTypeFromAttachmentTypeDto(attachmentType);
                _dbContext.ReportElementTypes.Add(attachmentTypeToPersist);
            }
            _dbContext.SaveChanges();

        }

        public void DeleteCertificationType(int certificationTypeId)
        {
            var foundReportElementType = _dbContext.ReportElementTypes
                .Single(r => r.ReportElementTypeId == certificationTypeId);
            _dbContext.ReportElementTypes.Remove(foundReportElementType);
            _dbContext.SaveChanges();
        }

        public void DeleteAttachmentType(int attachmentTypeId)
        {
            var foundReportElementType = _dbContext.ReportElementTypes
                .Single(r => r.ReportElementTypeId == attachmentTypeId);
            _dbContext.ReportElementTypes.Remove(foundReportElementType);
            _dbContext.SaveChanges();
        }

        public IEnumerable<AttachmentTypeDto> GetAttachmentTypes()
        {
            var attachmentTypes = new List<AttachmentTypeDto>();
            var attachmentReportElementCategoryId = _dbContext.ReportElementCategories
                .Single(r => r.Name == ReportElementCategoryName.Attachments.ToString()).ReportElementCategoryId;
            var foundReportElementTypes = _dbContext.ReportElementTypes
                .Include(c => c.CtsEventType)
                .Where(c => c.OrganizationRegulatoryProgramId == _orgRegProgramId
                        && c.ReportElementCategoryId == attachmentReportElementCategoryId)
                .ToList();
            foreach (var reportElementType in foundReportElementTypes)
            {
                var dto = _mapHelper.GetAttachmentTypeDtoFromReportElementType(reportElementType);
                attachmentTypes.Add(dto);
            }
            return attachmentTypes;
        }
    }
}
