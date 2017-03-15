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
using Linko.LinkoExchange.Core.Enum;

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

        public IEnumerable<ReportElementTypeDto> GetReportElementTypes(ReportElementCategoryName categoryName)
        {
            var reportElementTypes = new List<ReportElementTypeDto>();
            var reportElementCategoryId = _dbContext.ReportElementCategories
                .Single(r => r.Name == categoryName.ToString()).ReportElementCategoryId;

            var foundReportElementTypes = _dbContext.ReportElementTypes
                .Include(c => c.CtsEventType)
                .Where(c => c.OrganizationRegulatoryProgramId == _orgRegProgramId
                    && c.ReportElementCategoryId == reportElementCategoryId)
                .ToList();
            foreach (var reportElementType in foundReportElementTypes)
            {
                var dto = _mapHelper.GetReportElementTypeDtoFromReportElementType(reportElementType);
                reportElementTypes.Add(dto);
            }
            return reportElementTypes;
        }

        public void SaveReportElementType(ReportElementTypeDto reportElementType)
        {
            ReportElementType ReportElementTypeToPersist = null;
            if (reportElementType.ReportElementTypeID.HasValue && reportElementType.ReportElementTypeID.Value > 0)
            {
                //Update existing
                ReportElementTypeToPersist = _dbContext.ReportElementTypes.Single(c => c.ReportElementTypeId == reportElementType.ReportElementTypeID);
                ReportElementTypeToPersist = _mapHelper.GetReportElementTypeFromReportElementTypeDto(reportElementType, ReportElementTypeToPersist);
            }
            else
            {
                //Get new
                ReportElementTypeToPersist = _mapHelper.GetReportElementTypeFromReportElementTypeDto(reportElementType);
                _dbContext.ReportElementTypes.Add(ReportElementTypeToPersist);
            }
            _dbContext.SaveChanges();

        }

    
        public void DeleteReportElementType(int ReportElementTypeId)
        {
            var foundReportElementType = _dbContext.ReportElementTypes
                .Single(r => r.ReportElementTypeId == ReportElementTypeId);
            _dbContext.ReportElementTypes.Remove(foundReportElementType);
            _dbContext.SaveChanges();
        }
    }
}
