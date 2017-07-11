using System.Configuration;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NLog;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services;
using Linko.LinkoExchange.Services.TimeZone;
using Linko.LinkoExchange.Services.FileStore;
using Linko.LinkoExchange.Services.Cache;

namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class FileStoreServiceTests
    {
        FileStoreService _fileStoreService;
        Mock<ITimeZoneService> _timeZoneService = new Mock<ITimeZoneService>();
        Mock<ILogger> _logger = new Mock<ILogger>();
        Mock<IHttpContextService> _httpContext = new Mock<IHttpContextService>();
        Mock<ISettingService> _settingService = new Mock<ISettingService>();

        public FileStoreServiceTests()
        {
        }

        [TestInitialize]
        public void Initialize()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["LinkoExchangeContext"].ConnectionString;
            var connection = new LinkoExchangeContext(connectionString);
            _httpContext = new Mock<IHttpContextService>();
            _logger = new Mock<ILogger>();
            _timeZoneService = new Mock<ITimeZoneService>();
            var actualSettingService = new SettingService(connection, _logger.Object, new MapHelper());
            var actualTimeZoneService = new TimeZoneService(connection, actualSettingService, new MapHelper());

            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns("1");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns("authority");

            _fileStoreService = new FileStoreService(
                connection,
                new MapHelper(),
                _logger.Object,
                _httpContext.Object,
                actualTimeZoneService
            );
        }

        [TestMethod]
        public void GetFileStoreById()
        {
            var fileStore = _fileStoreService.GetFileStoreById(2);
        }

      
    }
}
