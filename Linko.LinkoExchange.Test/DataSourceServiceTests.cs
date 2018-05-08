using System;
using System.Configuration;
using System.Linq;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.DataSource;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.HttpContext;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services.TimeZone;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NLog;

namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class DataSourceServiceTests
    {
        #region fields

        private Mock<IHttpContextService> _httpContext;
        private Mock<ILogger> _logger;
        private Mock<IOrganizationService> _orgService;
        private Mock<ITimeZoneService> _timeZoneService;
        private Mock<ISettingService> _settingService;
        private DataSourceService _dataSourceService;

        #endregion

        [TestInitialize]
        public void Initialize()
        {
            var connectionString = ConfigurationManager.ConnectionStrings[name:"LinkoExchangeContext"].ConnectionString;
            var connection = new LinkoExchangeContext(nameOrConnectionString:connectionString);
            _httpContext = new Mock<IHttpContextService>();
            _logger = new Mock<ILogger>();
            _orgService = new Mock<IOrganizationService>();
            _timeZoneService = new Mock<ITimeZoneService>();
            _settingService = new Mock<ISettingService>();

            _httpContext.Setup(s => s.GetClaimValue(It.IsAny<string>())).Returns(value:"1");
            _orgService.Setup(s => s.GetAuthority(It.IsAny<int>()))
                       .Returns(value: new OrganizationRegulatoryProgramDto { OrganizationRegulatoryProgramId = 1, OrganizationId = 1000 });
            _timeZoneService.Setup(s => s.GetLocalizedDateTimeUsingThisTimeZoneId(It.IsAny<DateTime>(), It.IsAny<int>()))
                            .Returns((DateTime dt, int tzId) => dt);
            _settingService.Setup(s => s.GetOrganizationSettingValue(It.IsAny<int>(), SettingType.TimeZone)).Returns("6");

            _dataSourceService = new DataSourceService(dbContext:connection,
                                                       httpContext:_httpContext.Object,
                                                       mapHelper:new MapHelper(),
                                                       logger:_logger.Object,
                                                       orgService:_orgService.Object,
                                                       settings: _settingService.Object,
                                                       timeZoneService: _timeZoneService.Object);
        }

        [TestMethod]
        [ExpectedException(exceptionType:typeof(RuleViolationException))]
        public void SaveParameterGroup_MissingRequiredName_ShouldThrowException()
        {
            var dataSourceDto = new DataSourceDto();
            _dataSourceService.SaveDataSource(dataSourceDto);
        }

        [TestMethod]
        public void CreateReadUpdateDeleteDataSources_ShouldAllPass()
        {
            const string dataSourceName = "AutomationTest_DC_001";
            const int orpId = 1;
            DeleteDataSourceByNameIfExist(orpId:1, dataSourceName:dataSourceName);

            var dataSourceDto = new DataSourceDto
                                {
                                    Name = dataSourceName,
                                    Description = "Description",
                                    OrganizationRegulatoryProgramId = orpId
                                };
            var dataSourceId = _dataSourceService.SaveDataSource(dataSourceDto:dataSourceDto);
            Assert.IsTrue(condition:dataSourceId > 0);

            var savedDataSourceDto = _dataSourceService.GetDataSourceById(dataSourceId:dataSourceId);
            Assert.AreEqual(expected:dataSourceId, actual:savedDataSourceDto.DataSourceId);
            Assert.AreEqual(expected:dataSourceDto.Name, actual:savedDataSourceDto.Name);
            Assert.AreEqual(expected:dataSourceDto.Description, actual:savedDataSourceDto.Description);
            Assert.AreEqual(expected:dataSourceDto.OrganizationRegulatoryProgramId, actual:savedDataSourceDto.OrganizationRegulatoryProgramId);

            var resultDtos = _dataSourceService.GetDataSources(organizationRegulatoryProgramId:orpId);
            Assert.IsNotNull(value:resultDtos.FirstOrDefault(c => c.DataSourceId == dataSourceId));

            var dataSourceToModify = savedDataSourceDto;
            dataSourceToModify.Description = dataSourceToModify.Description + " modified";
            var modifiedDataSourceId = _dataSourceService.SaveDataSource(dataSourceDto:dataSourceToModify);
            Assert.AreEqual(expected:modifiedDataSourceId, actual:dataSourceId);

            var modifiedDataSourceDto = _dataSourceService.GetDataSourceById(dataSourceId:modifiedDataSourceId);
            Assert.AreEqual(expected:modifiedDataSourceDto.Description, actual:dataSourceToModify.Description);

            _dataSourceService.DeleteDataSource(dataSourceId:dataSourceId);
            Assert.IsNull(value:_dataSourceService.GetDataSourceById(dataSourceId:dataSourceId));
        }

        private void DeleteDataSourceByNameIfExist(int orpId, string dataSourceName)
        {
            var dataSource = _dataSourceService.GetDataSource(organizationRegulatoryProgramId:orpId, name:dataSourceName);
            if (dataSource?.DataSourceId != null && dataSource.DataSourceId.Value > 0)
            {
                _dataSourceService.DeleteDataSource(dataSourceId:dataSource.DataSourceId.Value);
            }
        }
    }
}