using System.Configuration;
using System.Linq;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.DataSource;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.HttpContext;
using Linko.LinkoExchange.Services.Mapping;
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
        private DataSourceService _dataSourceService;

        #endregion

        [TestInitialize]
        public void Initialize()
        {
            var connectionString = ConfigurationManager.ConnectionStrings[name:"LinkoExchangeContext"].ConnectionString;
            var connection = new LinkoExchangeContext(nameOrConnectionString:connectionString);
            _httpContext = new Mock<IHttpContextService>();
            _logger = new Mock<ILogger>();

            var actualTimeZoneService = new TimeZoneService(dbContext: connection,
                                                            settings: new SettingService(dbContext: connection, logger: _logger.Object, mapHelper: new MapHelper(),
                                                                                         cache: new Mock<IRequestCache>().Object, globalSettings: new Mock<IGlobalSettings>().Object),
                                                            mapHelper: new MapHelper(), appCache: new Mock<IApplicationCache>().Object, logger: _logger.Object);
            var actualSettings = new SettingService(dbContext: connection, logger: _logger.Object, mapHelper: new MapHelper(), cache: new Mock<IRequestCache>().Object,
                                                    globalSettings: new Mock<IGlobalSettings>().Object);

            _httpContext.Setup(s => s.GetClaimValue(It.IsAny<string>())).Returns(value:"1");

            _dataSourceService = new DataSourceService(dbContext:connection,
                                                       httpContext:_httpContext.Object,
                                                       mapHelper:new MapHelper(),
                                                       logger:_logger.Object,
                                                       settings: actualSettings,
                                                       timeZoneService: actualTimeZoneService);
        }

        [TestMethod]
        [ExpectedException(exceptionType:typeof(RuleViolationException))]
        public void SaveParameterGroup_MissingRequiredName_ShouldThrowException()
        {
            var dataSourceDto = new DataSourceDto();
            _dataSourceService.SaveDataSource(dataSourceDto);
        }

        [TestMethod]
        public void Create5DataSources()
        {
            const int orpId = 31;
            for (var i = 0; i < 5; i++)
            {
                var dataSourceName = "DataSource00" + i;
                DeleteDataSourceByNameIfExist(orpId:1, dataSourceName:dataSourceName);

                var dataSourceDto = new DataSourceDto
                                    {
                                        Name = dataSourceName,
                                        Description = "Description",
                                        OrganizationRegulatoryProgramId = orpId
                                    };
                var dataSourceId = _dataSourceService.SaveDataSource(dataSourceDto:dataSourceDto);
                Assert.IsTrue(condition:dataSourceId > 0);
            }
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

            var resultDtos = _dataSourceService.GetDataSources(organziationRegulatoryProgramId:orpId);
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
            var dataSource = _dataSourceService.GetDataSource(organziationRegulatoryProgramId:orpId, name:dataSourceName);
            if (dataSource?.DataSourceId != null && dataSource.DataSourceId.Value > 0)
            {
                _dataSourceService.DeleteDataSource(dataSourceId:dataSource.DataSourceId.Value);
            }
        }
    }
}