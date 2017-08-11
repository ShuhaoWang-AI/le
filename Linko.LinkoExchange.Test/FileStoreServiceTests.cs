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
using System.Linq;
using Linko.LinkoExchange.Core.Enum;

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
        public void CanUserExecuteApi_GetFileStoreById_AsAuthority_Included_In_Submitted_Report_True_Test()
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

            //update report package to submitted
            var reportPackage = _dbContext.ReportPackages
                .Single(rp => rp.ReportPackageId == 1);

            reportPackage.ReportStatusId = _dbContext.ReportStatuses.Single(rs => rs.Name == ReportStatusName.Submitted.ToString()).ReportStatusId;

            _dbContext.SaveChanges();

            var isAuthorized = _fileStoreService.CanUserExecuteApi("GetFileStoreById", fileStoreId);

            //Tear down - delete file store
            _dbContext.ReportFiles
                .RemoveRange(_dbContext.ReportFiles
                                .Where(rf => rf.FileStoreId == fileStoreId));

            _dbContext.SaveChanges();

            _fileStoreService.DeleteFileStore(fileStoreId);

            Assert.IsTrue(isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_GetFileStoreById_AsAuthority_Included_In_Repudiated_Report_True_Test()
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

            //update report package to submitted
            var reportPackage = _dbContext.ReportPackages
                .Single(rp => rp.ReportPackageId == 1);

            reportPackage.ReportStatusId = _dbContext.ReportStatuses.Single(rs => rs.Name == ReportStatusName.Repudiated.ToString()).ReportStatusId;

            _dbContext.SaveChanges();

            var isAuthorized = _fileStoreService.CanUserExecuteApi("GetFileStoreById", fileStoreId);

            //Tear down - delete file store
            _dbContext.ReportFiles
                .RemoveRange(_dbContext.ReportFiles
                                .Where(rf => rf.FileStoreId == fileStoreId));

            _dbContext.SaveChanges();

            _fileStoreService.DeleteFileStore(fileStoreId);

            Assert.IsTrue(isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_GetFileStoreById_AsAuthority_Included_In_Draft_Report_False_Test()
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

            //update report package to submitted
            var reportPackage = _dbContext.ReportPackages
                .Single(rp => rp.ReportPackageId == 1);

            reportPackage.ReportStatusId = _dbContext.ReportStatuses.Single(rs => rs.Name == ReportStatusName.Draft.ToString()).ReportStatusId;

            _dbContext.SaveChanges();

            var isAuthorized = _fileStoreService.CanUserExecuteApi("GetFileStoreById", fileStoreId);

            //Tear down - delete file store
            _dbContext.ReportFiles
                .RemoveRange(_dbContext.ReportFiles
                                .Where(rf => rf.FileStoreId == fileStoreId));

            _dbContext.SaveChanges();

            _fileStoreService.DeleteFileStore(fileStoreId);

            Assert.IsFalse(isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_GetFileStoreById_AsAuthority_Included_In_ReadyToSubmit_Report_False_Test()
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

            //update report package to submitted
            var reportPackage = _dbContext.ReportPackages
                .Single(rp => rp.ReportPackageId == 1);

            reportPackage.ReportStatusId = _dbContext.ReportStatuses.Single(rs => rs.Name == ReportStatusName.ReadyToSubmit.ToString()).ReportStatusId;

            _dbContext.SaveChanges();

            var isAuthorized = _fileStoreService.CanUserExecuteApi("GetFileStoreById", fileStoreId);

            //Tear down - delete file store
            _dbContext.ReportFiles
                .RemoveRange(_dbContext.ReportFiles
                                .Where(rf => rf.FileStoreId == fileStoreId));

            _dbContext.SaveChanges();

            _fileStoreService.DeleteFileStore(fileStoreId);

            Assert.IsFalse(isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_GetFileStoreById_AsAuthority_NotIncluded_In_Any_Report_True_Test()
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

            var isAuthorized = _fileStoreService.CanUserExecuteApi("GetFileStoreById", fileStoreId);

            _fileStoreService.DeleteFileStore(fileStoreId);

            Assert.IsFalse(isAuthorized);
        }

    }
}
