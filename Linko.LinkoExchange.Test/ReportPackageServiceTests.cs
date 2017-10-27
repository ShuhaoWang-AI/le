using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.AuditLog;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.CopyOfRecord;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Email;
using Linko.LinkoExchange.Services.HttpContext;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Program;
using Linko.LinkoExchange.Services.Report;
using Linko.LinkoExchange.Services.Sample;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services.TimeZone;
using Linko.LinkoExchange.Services.Unit;
using Linko.LinkoExchange.Services.User;
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
    public class ReportPackageServiceTests
    {
        #region fields

        private readonly Mock<ICopyOfRecordService> _copyOfRecordService = new Mock<ICopyOfRecordService>();
        private readonly Mock<ILinkoExchangeEmailService> _linkoExchangeEmailService = new Mock<ILinkoExchangeEmailService>();
        private readonly Mock<IRequestCache> _requestCache = new Mock<IRequestCache>();
        private readonly Mock<ISettingService> _settingService = new Mock<ISettingService>();
        private readonly Mock<IUserService> _userService = new Mock<IUserService>();

        private Mock<IAuditLogService> _auditLogService = new Mock<IAuditLogService>();
        private Mock<ICromerrAuditLogService> _cromerrService = new Mock<ICromerrAuditLogService>();
        private Mock<IHttpContextService> _httpContext = new Mock<IHttpContextService>();
        private Mock<ILogger> _logger = new Mock<ILogger>();
        private Mock<IApplicationCache> _mockAppCache = new Mock<IApplicationCache>();
        private Mock<IRequestCache> _mockRequestCache = new Mock<IRequestCache>();
        private Mock<ISessionCache> _mockSessionCache = new Mock<ISessionCache>();
        private Mock<IOrganizationService> _orgService = new Mock<IOrganizationService>();
        private Mock<IProgramService> _programService = new Mock<IProgramService>();
        private Mock<ITimeZoneService> _timeZoneService = new Mock<ITimeZoneService>();
        private LinkoExchangeContext _dbContext;
        private ReportPackageService _reportPackageService;

        #endregion

        [TestInitialize]
        public void Initialize()
        {
            var connectionString = ConfigurationManager.ConnectionStrings[name:"LinkoExchangeContext"].ConnectionString;
            _dbContext = new LinkoExchangeContext(nameOrConnectionString:connectionString);
            _httpContext = new Mock<IHttpContextService>();
            _orgService = new Mock<IOrganizationService>();
            _logger = new Mock<ILogger>();
            _timeZoneService = new Mock<ITimeZoneService>();

            _mockAppCache = new Mock<IApplicationCache>();
            _mockAppCache.Setup(c => c.Get("VolumeFlowRateLimitBasisId")).Returns(value:"3");
            _mockAppCache.Setup(c => c.Get("TimeZoneId-6")).Returns(value:"Eastern Standard Time");

            _mockRequestCache = new Mock<IRequestCache>();
            _mockRequestCache.Setup(c => c.GetValue("TimeZone-1")).Returns(value:"6");

            var cachedFlowUnits = new List<UnitDto>();
            cachedFlowUnits.Add(item:new UnitDto {UnitId = 1, Name = "test", Description = "blahblahblahblahblahblahblahblahblahblahblahblah"});
            cachedFlowUnits.Add(item:new UnitDto {UnitId = 2, Name = "test", Description = "blahblahblahblahblahblahblahblahblahblahblahblah"});
            cachedFlowUnits.Add(item:new UnitDto {UnitId = 3, Name = "test", Description = "blahblahblahblahblahblahblahblahblahblahblahblah"});
            cachedFlowUnits.Add(item:new UnitDto {UnitId = 4, Name = "test", Description = "blahblahblahblahblahblahblahblahblahblahblahblah"});
            cachedFlowUnits.Add(item:new UnitDto {UnitId = 5, Name = "test", Description = "blahblahblahblahblahblahblahblahblahblahblahblah"});
            _mockRequestCache.Setup(c => c.GetValue("GetFlowUnitsFromCommaDelimitedString-gpd,mgd")).Returns(value:cachedFlowUnits);

            var actualSettingService = new SettingService(dbContext:_dbContext, logger:_logger.Object, mapHelper:new MapHelper(), cache:_mockRequestCache.Object,
                                                          globalSettings:new Mock<IGlobalSettings>().Object);
            var actualTimeZoneService = new TimeZoneService(dbContext:_dbContext, settings:actualSettingService, mapHelper:new MapHelper(), appCache:_mockAppCache.Object);
            _httpContext.Setup(s => s.GetClaimValue(It.IsAny<string>())).Returns(value:"1");

            var authorityOrgRegProgramDto = new OrganizationRegulatoryProgramDto();
            authorityOrgRegProgramDto.OrganizationDto = new OrganizationDto();
            authorityOrgRegProgramDto.OrganizationRegulatoryProgramId = 1;
            authorityOrgRegProgramDto.OrganizationDto.OrganizationId = 1000;
            authorityOrgRegProgramDto.OrganizationDto.OrganizationName = "Axys Chemicals";
            authorityOrgRegProgramDto.OrganizationDto.AddressLine1 = "1232 Johnson St.";
            authorityOrgRegProgramDto.OrganizationDto.AddressLine2 = "PO Box 1234";
            authorityOrgRegProgramDto.OrganizationDto.CityName = "Gotham";
            authorityOrgRegProgramDto.OrganizationDto.ZipCode = "90210";

            _orgService.Setup(s => s.GetAuthority(It.IsAny<int>())).Returns(value:authorityOrgRegProgramDto);

            _settingService.Setup(s => s.GetOrganizationSettingValue(It.IsAny<int>(), SettingType.ReportRepudiatedDays)).Returns(value:"180");
            _settingService.Setup(s => s.GetOrganizationSettingValue(It.IsAny<int>(), SettingType.TimeZone)).Returns(value:"6");

            _settingService.Setup(s => s.GetOrgRegProgramSettingValue(It.IsAny<int>(), SettingType.EmailContactInfoName)).Returns(value:"Email C Name");
            _settingService.Setup(s => s.GetOrgRegProgramSettingValue(It.IsAny<int>(), SettingType.EmailContactInfoEmailAddress)).Returns(value:"contactemail@auth.com");
            _settingService.Setup(s => s.GetOrgRegProgramSettingValue(It.IsAny<int>(), SettingType.EmailContactInfoPhone)).Returns(value:"(555) 555-5555");

            var systemSettingLookup = new Dictionary<SystemSettingType, string>();
            systemSettingLookup.Add(key:SystemSettingType.SystemEmailEmailAddress, value:"donteventrytoreply@linkotechnology.com");
            systemSettingLookup.Add(key:SystemSettingType.SystemEmailFirstName, value:"Adam");
            systemSettingLookup.Add(key:SystemSettingType.SystemEmailLastName, value:"Adminsky");

            //systemSettingLookup.Add(SystemSettingType.EmailServer, "wtraxadc2.watertrax.local");
            systemSettingLookup.Add(key:SystemSettingType.EmailServer, value:"192.168.5.51");
            systemSettingLookup.Add(key:SystemSettingType.FileAvailableToAttachMaxAgeMonths, value:"999");
            _settingService.Setup(s => s.GetGlobalSettings()).Returns(value:systemSettingLookup);

            _requestCache.Setup(s => s.GetValue(CacheKey.Token)).Returns(value:"some_token_string");

            var actualUnitService = new UnitService(dbContext:_dbContext, mapHelper:new MapHelper(), logger:_logger.Object, httpContextService:_httpContext.Object,
                                                    timeZoneService:actualTimeZoneService, orgService:_orgService.Object, settingService:actualSettingService,
                                                    requestCache:_mockRequestCache.Object);
            var actualSampleService = new SampleService(dbContext:_dbContext, httpContext:_httpContext.Object, orgService:_orgService.Object, mapHelper:new MapHelper(),
                                                        logger:_logger.Object, timeZoneService:actualTimeZoneService, settings:actualSettingService, unitService:actualUnitService,
                                                        cache:_mockAppCache.Object);

            var actualAuditLogService = new EmailAuditLogService(linkoExchangeContext:_dbContext, requestCache:_requestCache.Object, mapHelper:new MapHelper(),
                                                                 logger:_logger.Object);

            //   var actualEmailService = new LinkoExchangeEmailService(_dbContext, actualAuditLogService, _programService.Object, _settingService.Object, _logger.Object);
            var actualCromerrService = new CromerrAuditLogService(linkoExchangeContext:_dbContext, mapHelper:new MapHelper(), httpContext:_httpContext.Object,
                                                                  logger:_logger.Object);
            var actualProgramService = new ProgramService(applicationDbContext:_dbContext, mapHelper:new MapHelper());

            _reportPackageService = new ReportPackageService(
                                                             programService:actualProgramService,
                                                             copyOfRecordService:_copyOfRecordService.Object,
                                                             timeZoneService:actualTimeZoneService,
                                                             logger:_logger.Object,
                                                             linkoExchangeContext:_dbContext,
                                                             httpContextService:_httpContext.Object,
                                                             userService:_userService.Object,
                                                             settingService:_settingService.Object,
                                                             orgService:_orgService.Object,
                                                             sampleService:actualSampleService,
                                                             mapHelper:new MapHelper(),
                                                             crommerAuditLogService:actualCromerrService,
                                                             organizationService:new Mock<IOrganizationService>().Object,
                                                             linkoExchangeEmailService:_linkoExchangeEmailService.Object
                                                            );
        }

        [TestMethod]
        public void DeleteReportPackage()
        {
            _reportPackageService.DeleteReportPackage(reportPackageId:14);

            //_reportPackageService.DeleteReportPackage(9);
            //_reportPackageService.DeleteReportPackage(10);
        }

        [TestMethod]
        public void CreateDraft()
        {
            var templateId = 1;
            var startDateTimeLocal = new DateTime(year:2017, month:4, day:20);
            var endDateTimeLocal = new DateTime(year:2017, month:4, day:28);
            var newId = _reportPackageService.CreateDraft(reportPackageTemplateId:templateId, startDateTimeLocal:startDateTimeLocal, endDateTimeLocal:endDateTimeLocal);
        }

        [TestMethod]
        public void CreateDraft_Using_Date_Plus_Time()
        {
            var templateId = 1;
            var startDateTimeLocal = new DateTime(year:2017, month:4, day:20, hour:17, minute:30, second:30, millisecond:30);
            var endDateTimeLocal = new DateTime(year:2017, month:4, day:20, hour:17, minute:30, second:30, millisecond:30);
            var newId = _reportPackageService.CreateDraft(reportPackageTemplateId:templateId, startDateTimeLocal:startDateTimeLocal, endDateTimeLocal:endDateTimeLocal);
        }

        [TestMethod]
        public void CreateDraft_Using_Empty_Template()
        {
            var emptyReportPackageTemplateId = 9;
            var startDateTimeLocal = new DateTime(year:2017, month:4, day:20);
            var endDateTimeLocal = new DateTime(year:2017, month:4, day:28);
            var newId = _reportPackageService.CreateDraft(reportPackageTemplateId:emptyReportPackageTemplateId, startDateTimeLocal:startDateTimeLocal,
                                                          endDateTimeLocal:endDateTimeLocal);
        }

        [TestMethod]
        public void CreateDraft_Using_Certification_Only__Template()
        {
            var emptyReportPackageTemplateId = 10;
            var startDateTimeLocal = new DateTime(year:2017, month:4, day:20);
            var endDateTimeLocal = new DateTime(year:2017, month:4, day:28);
            var newId = _reportPackageService.CreateDraft(reportPackageTemplateId:emptyReportPackageTemplateId, startDateTimeLocal:startDateTimeLocal,
                                                          endDateTimeLocal:endDateTimeLocal);
        }

        [TestMethod]
        public void GetReportPackage()
        {
            //Fetch existing
            var existingReportPackage = _reportPackageService.GetReportPackage(reportPackageId:13, isIncludeAssociatedElementData:false);
        }

        [TestMethod]
        [ExpectedException(exceptionType:typeof(UnauthorizedAccessException))]
        public void GetReportPackage_UnauthorizedAccessException()
        {
            //Fetch existing
            _httpContext.Setup(s => s.GetClaimValue(It.IsAny<string>())).Returns(value:"4");
            var existingReportPackage = _reportPackageService.GetReportPackage(reportPackageId:8, isIncludeAssociatedElementData:false);
        }

        [TestMethod]
        public void GetReportPackage_With_Associated_Element_Children()
        {
            //Fetch existing
            var existingReportPackage = _reportPackageService.GetReportPackage(reportPackageId:21, isIncludeAssociatedElementData:true);
        }

        [TestMethod]
        public void SaveReportPackage_Add_Samples()
        {
            //Fetch existing
            var existingReportPackage = _reportPackageService.GetReportPackage(reportPackageId:13, isIncludeAssociatedElementData:false);

            //Add sample associations
            existingReportPackage.SamplesAndResultsTypes = new List<ReportPackageElementTypeDto>();
            var sampleReportPackageElementType = new ReportPackageElementTypeDto {ReportPackageElementTypeId = 25};
            sampleReportPackageElementType.Samples = new List<SampleDto>();
            sampleReportPackageElementType.Samples.Add(item:new SampleDto {SampleId = 52});
            sampleReportPackageElementType.Samples.Add(item:new SampleDto {SampleId = 53});
            existingReportPackage.SamplesAndResultsTypes.Add(item:sampleReportPackageElementType);
            var existingId = _reportPackageService.SaveReportPackage(reportPackageDto:existingReportPackage, isUseTransaction:true);
        }

        [TestMethod]
        public void SaveReportPackage_Add_Files()
        {
            //Fetch existing
            var existingReportPackage = _reportPackageService.GetReportPackage(reportPackageId:15, isIncludeAssociatedElementData:true);

            existingReportPackage.Comments = "Comments .. test.";

            //Add sample associations
            existingReportPackage.SamplesAndResultsTypes = new List<ReportPackageElementTypeDto>();
            var sampleReportPackageElementType = new ReportPackageElementTypeDto {ReportPackageElementTypeId = 25};
            sampleReportPackageElementType.Samples = new List<SampleDto>();
            sampleReportPackageElementType.Samples.Add(item:new SampleDto {SampleId = 52});

            //sampleReportPackageElementType.Samples.Add(new SampleDto { SampleId = 53 });
            existingReportPackage.SamplesAndResultsTypes.Add(item:sampleReportPackageElementType);

            //Add attachment associations
            existingReportPackage.AttachmentTypes = new List<ReportPackageElementTypeDto>();
            var attachmentReportPackageElementType = new ReportPackageElementTypeDto {ReportPackageElementTypeId = 26};
            attachmentReportPackageElementType.FileStores = new List<FileStoreDto>();
            attachmentReportPackageElementType.FileStores.Add(item:new FileStoreDto {FileStoreId = 2});

            //filesReportPackageElementType.FileStores.Add(new FileStoreDto { FileStoreId = 4 });
            existingReportPackage.AttachmentTypes.Add(item:attachmentReportPackageElementType);

            //Add certification associations
            existingReportPackage.CertificationTypes = new List<ReportPackageElementTypeDto>();
            var certsReportPackageElementType = new ReportPackageElementTypeDto {ReportPackageElementTypeId = 34};
            certsReportPackageElementType.FileStores = new List<FileStoreDto>();
            certsReportPackageElementType.FileStores.Add(item:new FileStoreDto {FileStoreId = 2});

            //filesReportPackageElementType.FileStores.Add(new FileStoreDto { FileStoreId = 4 });
            existingReportPackage.CertificationTypes.Add(item:certsReportPackageElementType);

            var existingId = _reportPackageService.SaveReportPackage(reportPackageDto:existingReportPackage, isUseTransaction:true);
        }

        [TestMethod]
        public void UpdateStatus()
        {
            //Change status
            _reportPackageService.UpdateStatus(reportPackageId:8, reportStatus:ReportStatusName.Submitted, isUseTransaction:false);
        }

        [TestMethod]
        public void GetFilesForSelection()
        {
            var eligibleFiles = _reportPackageService.GetFilesForSelection(reportPackageElementTypeId:1);
        }

        [TestMethod]
        public void GetSamplesForSelection()
        {
            var originalCount = 78665;

            //68824 = "GetFlowUnitsFromCommaDelimitedString" commented out
            //64501 = optimized linq
            //60351 = logging removed + only checking for last submitted if associated with any reports
            //44859 = cached timezone
            //39136 = cached "flow" limit basis id
            //25011 = app caching time zone name fetch
            //16030 = GetFlowUnitsFromCommaDelimitedString caching List<UnitDto>

            var watch = Stopwatch.StartNew();

            var eligibleSamples = _reportPackageService.GetSamplesForSelection(reportPackageElementTypeId:1);

            watch.Stop();

            var elapsedMs = watch.ElapsedMilliseconds;

            var percentageImprovement = Math.Round(d:decimal.Divide(d1:originalCount - elapsedMs, d2:originalCount) * 100, decimals:2);
        }

        [TestMethod]
        public void GetReportPackagesByStatusName_Draft()
        {
            var filteredReportPackages = _reportPackageService.GetReportPackagesByStatusName(reportStatusName:ReportStatusName.RepudiatedPendingReview);
        }

        [TestMethod]
        public void RepudiateReport()
        {
            //RepudiationReasonId     Name
            //1                       I did not submit this report
            //2                       Report is missing a sample
            //3                       Report is missing a parameter
            //4                       Report has errors
            //5                       A hold time was exceeded
            //6                       Other(please comment)

            _reportPackageService.RepudiateReport(reportPackageId:21, repudiationReasonId:6, repudiationReasonName:"Other(please comment)", comments:"Technical error");
        }

        [TestMethod]
        public void ReviewSubmission()
        {
            _reportPackageService.ReviewSubmission(reportPackageId:8);
        }

        [TestMethod]
        public void ReviewRepudiation()
        {
            _reportPackageService.ReviewRepudiation(reportPackageId:8, comments:"This repudiation has been reviewed!");
        }

        [TestMethod]
        public void Test_CreatePDF()
        {
            var templateId = 1;
            var startDateTimeLocal = new DateTime(year:2017, month:4, day:20);
            var endDateTimeLocal = new DateTime(year:2018, month:4, day:28);
            var reportPackageId = _reportPackageService.CreateDraft(reportPackageTemplateId:templateId, startDateTimeLocal:startDateTimeLocal, endDateTimeLocal:endDateTimeLocal);
        }

        [TestMethod]
        public void UpdateLastSentDateTime()
        {
            _reportPackageService.UpdateLastSentDateTime(reportPackageId:1, sentDateTime:DateTimeOffset.Now, lastSenderUserId:1, lastSenderFirstName:"Testing First Name");
        }

        [TestMethod]
        public void IsSimilarReportPackageSubmittedAfter()
        {
            var result = _reportPackageService.IsSimilarReportPackageSubmittedAfter(reportPackageId:4);
        }

        [TestMethod]
        public void CanRepudiateReportPackage()
        {
            var result = _reportPackageService.CanRepudiateReportPackage(reportPackageId:4);
        }

        [TestMethod]
        public void CanUserExecuteApi_GetReportPackage_AsAuthority_True_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns(value:"authority");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns(value:"1");

            var reportPackageId = 1;
            var isAuthorized = _reportPackageService.CanUserExecuteApi("GetReportPackage", reportPackageId);

            Assert.IsTrue(condition:isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_GetReportPackage_AsAuthority_False_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns(value:"authority");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns(value:"99");

            var reportPackageId = 1;
            var isAuthorized = _reportPackageService.CanUserExecuteApi("GetReportPackage", reportPackageId);

            Assert.IsFalse(condition:isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_GetReportPackage_AsIndustry_True_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns(value:"industry");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns(value:"13");

            var reportPackageId = 1;
            var isAuthorized = _reportPackageService.CanUserExecuteApi("GetReportPackage", reportPackageId);

            Assert.IsTrue(condition:isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_GetReportPackage_AsIndustry_False_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns(value:"industry");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns(value:"99");

            var reportPackageId = 1;
            var isAuthorized = _reportPackageService.CanUserExecuteApi("GetReportPackage", reportPackageId);

            Assert.IsFalse(condition:isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_SignAndSubmitReportPackage_Authorized_Test()
        {
            //Setup mocks
            var orgRegProgramId = 13;
            var userProfileId = 4;
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns(value:orgRegProgramId.ToString());
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.UserProfileId)).Returns(value:userProfileId.ToString());

            //Set flags of the user to allow authorization
            var orgRegProgramUser = _dbContext.OrganizationRegulatoryProgramUsers
                                              .Single(orpu => orpu.UserProfileId == userProfileId
                                                              && orpu.OrganizationRegulatoryProgramId == orgRegProgramId);

            orgRegProgramUser.IsSignatory = true;
            orgRegProgramUser.IsEnabled = true;
            orgRegProgramUser.IsRemoved = false;

            var reportPackageId = 1;

            var isAuthorized = _reportPackageService.CanUserExecuteApi("SignAndSubmitReportPackage", reportPackageId);

            Assert.IsTrue(condition:isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_SignAndSubmitReportPackage_Unauthorized_NotSignatory_Test()
        {
            //Setup mocks
            var orgRegProgramId = 13;
            var userProfileId = 4;
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns(value:orgRegProgramId.ToString());
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.UserProfileId)).Returns(value:userProfileId.ToString());

            //Set flags of the user to block authorization
            var orgRegProgramUser = _dbContext.OrganizationRegulatoryProgramUsers
                                              .Single(orpu => orpu.UserProfileId == userProfileId
                                                              && orpu.OrganizationRegulatoryProgramId == orgRegProgramId);

            orgRegProgramUser.IsSignatory = false; //UNAUTHORIZED
            orgRegProgramUser.IsEnabled = true;
            orgRegProgramUser.IsRemoved = false;

            var reportPackageId = 1;

            var isAuthorized = _reportPackageService.CanUserExecuteApi("SignAndSubmitReportPackage", reportPackageId);

            Assert.IsFalse(condition:isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_SignAndSubmitReportPackage_Unauthorized_Disabled_Test()
        {
            //Setup mocks
            var orgRegProgramId = 13;
            var userProfileId = 4;
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns(value:orgRegProgramId.ToString());
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.UserProfileId)).Returns(value:userProfileId.ToString());

            //Set flags of the user to block authorization
            var orgRegProgramUser = _dbContext.OrganizationRegulatoryProgramUsers
                                              .Single(orpu => orpu.UserProfileId == userProfileId
                                                              && orpu.OrganizationRegulatoryProgramId == orgRegProgramId);

            orgRegProgramUser.IsSignatory = true;
            orgRegProgramUser.IsEnabled = false; //UNAUTHORIZED
            orgRegProgramUser.IsRemoved = false;

            var reportPackageId = 1;

            var isAuthorized = _reportPackageService.CanUserExecuteApi("SignAndSubmitReportPackage", reportPackageId);

            Assert.IsFalse(condition:isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_SignAndSubmitReportPackage_Unauthorized_Removed_Test()
        {
            //Setup mocks
            var orgRegProgramId = 13;
            var userProfileId = 4;
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns(value:orgRegProgramId.ToString());
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.UserProfileId)).Returns(value:userProfileId.ToString());

            //Set flags of the user to block authorization
            var orgRegProgramUser = _dbContext.OrganizationRegulatoryProgramUsers
                                              .Single(orpu => orpu.UserProfileId == userProfileId
                                                              && orpu.OrganizationRegulatoryProgramId == orgRegProgramId);

            orgRegProgramUser.IsSignatory = true;
            orgRegProgramUser.IsEnabled = true;
            orgRegProgramUser.IsRemoved = true; //UNAUTHORIZED

            var reportPackageId = 1;

            var isAuthorized = _reportPackageService.CanUserExecuteApi("SignAndSubmitReportPackage", reportPackageId);

            Assert.IsFalse(condition:isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_SignAndSubmitReportPackage_Unauthorized_User_Does_Not_Exist_Test()
        {
            //Setup mocks
            var orgRegProgramId = 13;
            var userProfileId = -99; //WRONG USER PROFILE
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns(value:orgRegProgramId.ToString());
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.UserProfileId)).Returns(value:userProfileId.ToString());

            var reportPackageId = 1;

            var isAuthorized = _reportPackageService.CanUserExecuteApi("SignAndSubmitReportPackage", reportPackageId);

            Assert.IsFalse(condition:isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_SignAndSubmitReportPackage_Unauthorized_Invalid_User_OrgRegProgram_Combination_Test()
        {
            //Setup mocks
            var orgRegProgramId = 13;
            var userProfileId = 2;
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns(value:orgRegProgramId.ToString());
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.UserProfileId)).Returns(value:userProfileId.ToString());

            var reportPackageId = 1;

            var isAuthorized = _reportPackageService.CanUserExecuteApi("SignAndSubmitReportPackage", reportPackageId);

            Assert.IsFalse(condition:isAuthorized);
        }
    }
}