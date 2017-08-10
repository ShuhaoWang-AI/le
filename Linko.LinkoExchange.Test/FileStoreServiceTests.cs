using System.Configuration;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NLog;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.TimeZone;
using Linko.LinkoExchange.Services.FileStore;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.HttpContext;
using Linko.LinkoExchange.Services.Dto;
using System;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class FileStoreServiceTests
    {
        LinkoExchangeContext _dbContext;
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
            _dbContext = new LinkoExchangeContext(connectionString);
            _httpContext = new Mock<IHttpContextService>();
            _logger = new Mock<ILogger>();
            _timeZoneService = new Mock<ITimeZoneService>();
            var actualSettingService = new SettingService(_dbContext, _logger.Object, new MapHelper(), new Mock<IRequestCache>().Object, new Mock<IGlobalSettings>().Object);
            var actualTimeZoneService = new TimeZoneService(_dbContext, actualSettingService, new MapHelper(), new Mock<IApplicationCache>().Object);

            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns("1");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns("authority");

            _fileStoreService = new FileStoreService(
                _dbContext,
                new MapHelper(),
                _logger.Object,
                _httpContext.Object,
                actualTimeZoneService
            );
        }

        [TestMethod]
        public void GetFileStoreById()
        {
            var fileStore = _fileStoreService.GetFileStoreById(1);
        }

        [TestMethod]
        public void CanUserExecuteApi_UpdateFileStore_True_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns("industry");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns("1");

            int fileStoreId = 3;
            var isAuthorized = _fileStoreService.CanUserExecuteApi("UpdateFileStore", fileStoreId);

            Assert.IsTrue(isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_UpdateFileStore_False_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns("industry");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns("2");

            int fileStoreId = 3;
            var isAuthorized = _fileStoreService.CanUserExecuteApi("UpdateFileStore", fileStoreId);

            Assert.IsFalse(isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_GetFileStoreById_AsIndustry_True_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns("industry");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns("1");

            int fileStoreId = 3;
            var isAuthorized = _fileStoreService.CanUserExecuteApi("GetFileStoreById", fileStoreId);

            Assert.IsTrue(isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_GetFileStoreById_AsIndustry_False_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns("industry");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns("2");

            int fileStoreId = 3;
            var isAuthorized = _fileStoreService.CanUserExecuteApi("GetFileStoreById", fileStoreId);

            Assert.IsFalse(isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_GetFileStoreById_CorrectAuthority_NotIncludedInReport_False_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns("authority");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns("1");

            int fileStoreId = 3;
            var isAuthorized = _fileStoreService.CanUserExecuteApi("GetFileStoreById", fileStoreId);

            Assert.IsFalse(isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_GetFileStoreById_AsAuthority_False_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns("authority");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns("2");

            int fileStoreId = 3;
            var isAuthorized = _fileStoreService.CanUserExecuteApi("GetFileStoreById", fileStoreId);

            Assert.IsFalse(isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_GetFileStoreById_AsAuthority_True_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns("authority");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns("1");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.UserProfileId)).Returns("1");
            
            //Create file object in the database
            var fileStoreDto = new FileStoreDto()
            {
                Name = "Test File Store Name",
                OriginalFileName = "test_filestore.docx",
                SizeByte = 1.1,
                FileTypeId = 1,
                ReportElementTypeId = 1,
                ReportElementTypeName = "RPET Name",
                OrganizationRegulatoryProgramId = 1,
                UploadDateTimeLocal = DateTime.Now,
                UploaderUserId = 1,
                Data = new byte[1]
            };

            var fileStoreId = _fileStoreService.CreateFileStore(fileStoreDto);

            _dbContext.ReportFiles.Add(new ReportFile() { ReportPackageElementTypeId = 1, FileStoreId = fileStoreId });
            _dbContext.SaveChanges();

            var isAuthorized = _fileStoreService.CanUserExecuteApi("GetFileStoreById", fileStoreId);

            Assert.IsTrue(isAuthorized);
        }

    }
}
