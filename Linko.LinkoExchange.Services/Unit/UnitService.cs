using System;
using System.Collections.Generic;
using System.Linq;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.Report;
using Linko.LinkoExchange.Services.TimeZone;
using NLog;

namespace Linko.LinkoExchange.Services.Unit
{
    public class UnitService : IUnitService
    {
        private readonly LinkoExchangeContext _dbContext;
        private readonly IMapHelper _mapHelper;
        private readonly ILogger _logger;
        private readonly IHttpContextService _httpContextService;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IReportPackageService _reportPackageService;

        public UnitService(
            LinkoExchangeContext dbContext,
            IMapHelper mapHelper,
            ILogger logger,
            IHttpContextService httpContextService,
            ITimeZoneService timeZoneService,
            IReportPackageService reportPackageService)
        {
            if (dbContext == null)
            {
                throw new ArgumentNullException(nameof(dbContext));
            }

            if (mapHelper == null)
            {
                throw new ArgumentNullException(nameof(mapHelper));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (httpContextService == null)
            {
                throw new ArgumentNullException(nameof(httpContextService));
            }

            if (timeZoneService == null)
            {
                throw new ArgumentNullException(nameof(timeZoneService));
            }

            if (reportPackageService == null)
            {
                throw new ArgumentNullException(nameof(reportPackageService));
            }

            _dbContext = dbContext;
            _mapHelper = mapHelper;
            _logger = logger;
            _httpContextService = httpContextService;
            _timeZoneService = timeZoneService;
            _reportPackageService = reportPackageService;
        }

        public List<UnitDto> GetFlowUnits()
        {
            _logger.Info("Enter UnitService.GetFlowUnits.");
            var units = _dbContext.Units.Where(i => i.IsFlowUnit == true).ToList();

            var unitsDtos = units.Select(i => _mapHelper.GetUnitDtoFromUnit(i)).ToList();

            //TODO: to upodate teh last modification time and last modifier   
            //foreach (var unit in unitsDtos)
            //{ 
            //    unit.LastModificationDateTimeUtc =
            //}
            _logger.Info("Leave UnitService.GetFlowUnits.");

            return unitsDtos;
        }
    }
}