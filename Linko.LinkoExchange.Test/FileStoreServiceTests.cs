using System;
using System.Configuration;
using System.Linq;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.FileStore;
using Linko.LinkoExchange.Services.HttpContext;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services.TimeZone;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NLog;

// ReSharper disable ArgumentsStyleNamedExpression
// ReSharper disable ArgumentsStyleOther
// ReSharper disable ArgumentsStyleLiteral
// ReSharper disable ArgumentsStyleStringLiteral

namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class FileStoreServiceTests
    {
        #region fields

        private LinkoExchangeContext _dbContext;
        private FileStoreService _fileStoreService;
        private Mock<IHttpContextService> _httpContext = new Mock<IHttpContextService>();
        private Mock<ILogger> _logger = new Mock<ILogger>();
        private Mock<ISettingService> _settingService = new Mock<ISettingService>();
        private Mock<ITimeZoneService> _timeZoneService = new Mock<ITimeZoneService>();

        #endregion

        [TestInitialize]
        public void Initialize()
        {
            var connectionString = ConfigurationManager.ConnectionStrings[name:"LinkoExchangeContext"].ConnectionString;
            _dbContext = new LinkoExchangeContext(nameOrConnectionString:connectionString);
            _httpContext = new Mock<IHttpContextService>();
            _logger = new Mock<ILogger>();
            _timeZoneService = new Mock<ITimeZoneService>();
            var actualSettingService = new SettingService(dbContext:_dbContext, logger:_logger.Object, mapHelper:new MapHelper(), cache:new Mock<IRequestCache>().Object,
                                                          globalSettings:new Mock<IGlobalSettings>().Object);
            var actualTimeZoneService = new TimeZoneService(dbContext:_dbContext, settings:actualSettingService, mapHelper:new MapHelper(),
                                                            appCache:new Mock<IApplicationCache>().Object, logger:_logger.Object);

            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns(value:"1");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns(value:"authority");

            _fileStoreService = new FileStoreService(
                                                     dbContext:_dbContext,
                                                     mapHelper:new MapHelper(),
                                                     logger:_logger.Object,
                                                     httpContextService:_httpContext.Object,
                                                     timeZoneService:actualTimeZoneService
                                                    );
        }

        [TestMethod]
        public void GetFileStoreById()
        {
            var fileStore = _fileStoreService.GetFileStoreById(fileStoreId:1);
        }

        [TestMethod]
        public void CanUserExecuteApi_UpdateFileStore_True_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns(value:"industry");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns(value:"1");

            var fileStoreId = 3;
            var isAuthorized = _fileStoreService.CanUserExecuteApi("UpdateFileStore", fileStoreId);

            Assert.IsTrue(condition:isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_UpdateFileStore_False_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns(value:"industry");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns(value:"2");

            var fileStoreId = 3;
            var isAuthorized = _fileStoreService.CanUserExecuteApi("UpdateFileStore", fileStoreId);

            Assert.IsFalse(condition:isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_GetFileStoreById_AsIndustry_True_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns(value:"industry");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns(value:"1");

            var fileStoreId = 3;
            var isAuthorized = _fileStoreService.CanUserExecuteApi("GetFileStoreById", fileStoreId);

            Assert.IsTrue(condition:isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_GetFileStoreById_AsIndustry_False_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns(value:"industry");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns(value:"2");

            var fileStoreId = 3;
            var isAuthorized = _fileStoreService.CanUserExecuteApi("GetFileStoreById", fileStoreId);

            Assert.IsFalse(condition:isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_GetFileStoreById_CorrectAuthority_NotIncludedInReport_False_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns(value:"authority");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns(value:"1");

            var fileStoreId = 3;
            var isAuthorized = _fileStoreService.CanUserExecuteApi("GetFileStoreById", fileStoreId);

            Assert.IsFalse(condition:isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_GetFileStoreById_AsAuthority_Included_In_Submitted_Report_True_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns(value:"authority");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns(value:"1");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.UserProfileId)).Returns(value:"1");

            //Create file object in the database
            var fileStoreDto = new FileStoreDto
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

            var fileStoreId = _fileStoreService.CreateFileStore(fileStoreDto:fileStoreDto);

            _dbContext.ReportFiles.Add(entity:new ReportFile {ReportPackageElementTypeId = 1, FileStoreId = fileStoreId});

            //update report package to submitted
            var reportPackage = _dbContext.ReportPackages.Single(rp => rp.ReportPackageId == 1);

            reportPackage.ReportStatusId = _dbContext.ReportStatuses.Single(rs => rs.Name == ReportStatusName.Submitted.ToString()).ReportStatusId;

            _dbContext.SaveChanges();

            var isAuthorized = _fileStoreService.CanUserExecuteApi("GetFileStoreById", fileStoreId);

            //Tear down - delete file store
            _dbContext.ReportFiles.RemoveRange(entities:_dbContext.ReportFiles.Where(rf => rf.FileStoreId == fileStoreId));

            _dbContext.SaveChanges();

            _fileStoreService.DeleteFileStore(fileStoreId:fileStoreId);

            Assert.IsTrue(condition:isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_GetFileStoreById_AsAuthority_Included_In_Repudiated_Report_True_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns(value:"authority");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns(value:"1");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.UserProfileId)).Returns(value:"1");

            //Create file object in the database
            var fileStoreDto = new FileStoreDto
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

            var fileStoreId = _fileStoreService.CreateFileStore(fileStoreDto:fileStoreDto);

            _dbContext.ReportFiles.Add(entity:new ReportFile {ReportPackageElementTypeId = 1, FileStoreId = fileStoreId});

            //update report package to submitted
            var reportPackage = _dbContext.ReportPackages.Single(rp => rp.ReportPackageId == 1);

            reportPackage.ReportStatusId = _dbContext.ReportStatuses.Single(rs => rs.Name == ReportStatusName.Repudiated.ToString()).ReportStatusId;

            _dbContext.SaveChanges();

            var isAuthorized = _fileStoreService.CanUserExecuteApi("GetFileStoreById", fileStoreId);

            //Tear down - delete file store
            _dbContext.ReportFiles.RemoveRange(entities:_dbContext.ReportFiles.Where(rf => rf.FileStoreId == fileStoreId));

            _dbContext.SaveChanges();

            _fileStoreService.DeleteFileStore(fileStoreId:fileStoreId);

            Assert.IsTrue(condition:isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_GetFileStoreById_AsAuthority_Included_In_Draft_Report_False_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns(value:"authority");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns(value:"1");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.UserProfileId)).Returns(value:"1");

            //Create file object in the database
            var fileStoreDto = new FileStoreDto
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

            var fileStoreId = _fileStoreService.CreateFileStore(fileStoreDto:fileStoreDto);

            _dbContext.ReportFiles.Add(entity:new ReportFile {ReportPackageElementTypeId = 1, FileStoreId = fileStoreId});

            //update report package to submitted
            var reportPackage = _dbContext.ReportPackages.Single(rp => rp.ReportPackageId == 1);

            reportPackage.ReportStatusId = _dbContext.ReportStatuses.Single(rs => rs.Name == ReportStatusName.Draft.ToString()).ReportStatusId;

            _dbContext.SaveChanges();

            var isAuthorized = _fileStoreService.CanUserExecuteApi("GetFileStoreById", fileStoreId);

            //Tear down - delete file store
            _dbContext.ReportFiles.RemoveRange(entities:_dbContext.ReportFiles.Where(rf => rf.FileStoreId == fileStoreId));

            _dbContext.SaveChanges();

            _fileStoreService.DeleteFileStore(fileStoreId:fileStoreId);

            Assert.IsFalse(condition:isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_GetFileStoreById_AsAuthority_Included_In_ReadyToSubmit_Report_False_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns(value:"authority");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns(value:"1");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.UserProfileId)).Returns(value:"1");

            //Create file object in the database
            var fileStoreDto = new FileStoreDto
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

            var fileStoreId = _fileStoreService.CreateFileStore(fileStoreDto:fileStoreDto);

            _dbContext.ReportFiles.Add(entity:new ReportFile {ReportPackageElementTypeId = 1, FileStoreId = fileStoreId});

            //update report package to submitted
            var reportPackage = _dbContext.ReportPackages.Single(rp => rp.ReportPackageId == 1);

            reportPackage.ReportStatusId = _dbContext.ReportStatuses.Single(rs => rs.Name == ReportStatusName.ReadyToSubmit.ToString()).ReportStatusId;

            _dbContext.SaveChanges();

            var isAuthorized = _fileStoreService.CanUserExecuteApi("GetFileStoreById", fileStoreId);

            //Tear down - delete file store
            _dbContext.ReportFiles.RemoveRange(entities:_dbContext.ReportFiles.Where(rf => rf.FileStoreId == fileStoreId));

            _dbContext.SaveChanges();

            _fileStoreService.DeleteFileStore(fileStoreId:fileStoreId);

            Assert.IsFalse(condition:isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_GetFileStoreById_AsAuthority_NotIncluded_In_Any_Report_True_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns(value:"authority");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns(value:"1");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.UserProfileId)).Returns(value:"1");

            //Create file object in the database
            var fileStoreDto = new FileStoreDto
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

            var fileStoreId = _fileStoreService.CreateFileStore(fileStoreDto:fileStoreDto);

            var isAuthorized = _fileStoreService.CanUserExecuteApi("GetFileStoreById", fileStoreId);

            _fileStoreService.DeleteFileStore(fileStoreId:fileStoreId);

            Assert.IsFalse(condition:isAuthorized);
        }
    }
}