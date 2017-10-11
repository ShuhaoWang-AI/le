using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
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
using Linko.LinkoExchange.Services.QuestionAnswer;
using Linko.LinkoExchange.Services.Report;
using Linko.LinkoExchange.Services.Sample;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services.TimeZone;
using Linko.LinkoExchange.Services.User;
using Microsoft.AspNet.Identity;
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
    public class CopyOfRecordServiceTests
    {
        #region fields

        private readonly Mock<ICromerrAuditLogService> _cromerrService = new Mock<ICromerrAuditLogService>();
        private readonly Mock<ICromerrAuditLogService> _crommerAuditLogService = new Mock<ICromerrAuditLogService>();

        private readonly Mock<IHttpContextService> _httpContext = new Mock<IHttpContextService>();
        private readonly Mock<ILinkoExchangeEmailService> _linkoExchangeEmailService = new Mock<ILinkoExchangeEmailService>();
        private readonly Mock<ILogger> _logger = new Mock<ILogger>();
        private readonly MapHelper _mapHeper = new MapHelper();
        private readonly Mock<IOrganizationService> _orgService = new Mock<IOrganizationService>();
        private readonly IProgramService _programService = Mock.Of<IProgramService>();

        private readonly Mock<IReportPackageService> _reprotPackageService = new Mock<IReportPackageService>();
        private readonly Mock<IRequestCache> _requestCache = new Mock<IRequestCache>();
        private readonly Mock<ISampleService> _sampleService = new Mock<ISampleService>();
        private readonly Mock<ISettingService> _settingService = new Mock<ISettingService>();
        private readonly ISettingService _settService = Mock.Of<ISettingService>();
        private TimeZoneService _actualTimeZoneService;

        private Mock<IAuditLogEntry> _auditLoger = new Mock<IAuditLogEntry>();
        private CopyOfRecordService _copyOrRecordService;
        private LinkoExchangeContext _dbContext;
        private Mock<IPasswordHasher> _passwordHasher = new Mock<IPasswordHasher>();
        private Mock<IQuestionAnswerService> _questionAnswerServices = new Mock<IQuestionAnswerService>();
        private Mock<ISessionCache> _sessionCache = new Mock<ISessionCache>();
        private IUserService _userService;

        #endregion

        [TestInitialize]
        public void Init()
        {
            var connectionString = ConfigurationManager.ConnectionStrings[name:"LinkoExchangeContext"].ConnectionString;

            _dbContext = new LinkoExchangeContext(nameOrConnectionString:connectionString);

            var settServiceMock = Mock.Get(mocked:_settService);
            settServiceMock.Setup(i => i.GetOrganizationSettingValue(It.IsAny<int>(), It.IsAny<SettingType>())).Returns(value:"3");

            _settingService.Setup(i => i.GetOrganizationSettingValue(It.IsAny<int>(), It.IsAny<SettingType>())).Returns(value:"1");

            _actualTimeZoneService = new TimeZoneService(dbContext:_dbContext, settings:_settService, mapHelper:new MapHelper(), appCache:new Mock<IApplicationCache>().Object);
            _httpContext.Setup(s => s.GetClaimValue(It.IsAny<string>())).Returns(value:"1");

            _userService = new UserService(dbContext:_dbContext, httpContext:_httpContext.Object,
                                           settingService:_settingService.Object, orgService:_orgService.Object, requestCache:_requestCache.Object,
                                           timeZones:_actualTimeZoneService,
                                           logService:_logger.Object, mapHelper:_mapHeper, crommerAuditLogService:_crommerAuditLogService.Object,
                                           linkoExchangeEmailService:_linkoExchangeEmailService.Object);

            IDigitalSignatureManager certificateDigitalSignatureManager =
                new CertificateDigitalSignatureManager(dbContext:_dbContext, logger:_logger.Object, httpContextService:_httpContext.Object);

            _copyOrRecordService = new CopyOfRecordService(dbContext:_dbContext, logger:_logger.Object, digitalSignatureManager:certificateDigitalSignatureManager);
        }

        [TestMethod]
        public void Create_Cor_success()
        {
            //set 
            var rnd = new Random();
            var rptId = rnd.Next(maxValue:int.MaxValue);
            var attachments = GetMockAttachmentFiles();
            var copyOfRecordPdfFile = GetCopyOfRecordPdfFile();
            var reportPackageCopyOfRecordDataXml = GetReportPackageCopyOfRecordDataXml();

            _reprotPackageService.Setup(i => i.GetReportPackageCopyOfRecordDataXmlFile(It.IsAny<ReportPackageDto>()))
                                 .Returns(value:reportPackageCopyOfRecordDataXml);
            _reprotPackageService.Setup(i => i.GetReportPackageCopyOfRecordPdfFile(It.IsAny<ReportPackageDto>()))
                                 .Returns(value:copyOfRecordPdfFile);
            var reportPackageDto = new ReportPackageDto();

            var attachmentFiles = attachments;
            var copyOfRecordDataXmlFileInfo = _reprotPackageService.Object.GetReportPackageCopyOfRecordDataXmlFile(reportPackageDto:reportPackageDto);
            var copyOfRecordPdfInfo = _reprotPackageService.Object.GetReportPackageCopyOfRecordPdfFile(reportPackageDto:reportPackageDto);

            _copyOrRecordService.CreateCopyOfRecordForReportPackage(reportPackageId:rptId, attachments:attachmentFiles, copyOfRecordPdfFileDto:copyOfRecordPdfInfo,
                                                                    copyOfRecordDataXmlFileDto:copyOfRecordDataXmlFileInfo);
        }

        [TestMethod]
        public void Verify_Cor_success()
        {
            var programDto = Mock.Of<OrganizationRegulatoryProgramDto>();

            var programMock = Mock.Get(mocked:_programService);
            programMock.Setup(i => i.GetOrganizationRegulatoryProgram(It.IsAny<int>()))
                       .Returns(value:programDto);

            IReportPackageService reportPackageService =
                new ReportPackageService(programService:_programService,
                                         copyOfRecordService:_copyOrRecordService,
                                         timeZoneService:_actualTimeZoneService,
                                         logger:_logger.Object,
                                         linkoExchangeContext:_dbContext,
                                         httpContextService:_httpContext.Object,
                                         userService:_userService,
                                         settingService:_settService,
                                         orgService:_orgService.Object,
                                         sampleService:_sampleService.Object,
                                         mapHelper:_mapHeper,
                                         crommerAuditLogService:_cromerrService.Object,
                                         organizationService:new Mock<IOrganizationService>().Object,
                                         linkoExchangeEmailService:_linkoExchangeEmailService.Object);

            var reportPackageId = 527466233;
            var validResult = reportPackageService.VerifyCopyOfRecord(reportPackageId:reportPackageId);

            Assert.IsTrue(condition:validResult.Valid);
        }

        [TestMethod]
        public void Verify_intacted_data_return_true()
        {
            var programDto = Mock.Of<OrganizationRegulatoryProgramDto>();
            var programMock = Mock.Get(mocked:_programService);
            programMock.Setup(i => i.GetOrganizationRegulatoryProgram(It.IsAny<int>())).Returns(value:programDto);

            IReportPackageService reportPackageService =
                new ReportPackageService(programService:_programService,
                                         copyOfRecordService:_copyOrRecordService,
                                         timeZoneService:_actualTimeZoneService,
                                         logger:_logger.Object,
                                         linkoExchangeContext:_dbContext,
                                         httpContextService:_httpContext.Object,
                                         userService:_userService,
                                         settingService:_settService,
                                         orgService:_orgService.Object,
                                         sampleService:_sampleService.Object,
                                         mapHelper:_mapHeper,
                                         crommerAuditLogService:_cromerrService.Object,
                                         organizationService:new Mock<IOrganizationService>().Object,
                                         linkoExchangeEmailService:_linkoExchangeEmailService.Object);

            var reportPackageId = 527466233;
            var copyOfRecordDto = reportPackageService.GetCopyOfRecordByReportPackageId(reportPackageId:reportPackageId);
            var certificateDigitalSignatureManager = new CertificateDigitalSignatureManager(dbContext:_dbContext, logger:_logger.Object, httpContextService:_httpContext.Object);
            var result = certificateDigitalSignatureManager.VerifySignature(currentSignatureStr:copyOfRecordDto.Signature, dataToVerify:copyOfRecordDto.Data, copyOfRecordCertificateId: copyOfRecordDto.CopyOfRecordCertificateId);

            Assert.IsTrue(condition:result);
        }

        [TestMethod]
        public void Verify_wrong_signature_1_return_false()
        {
            var programDto = Mock.Of<OrganizationRegulatoryProgramDto>();

            var programMock = Mock.Get(mocked:_programService);
            programMock.Setup(i => i.GetOrganizationRegulatoryProgram(It.IsAny<int>()))
                       .Returns(value:programDto);

            IReportPackageService reportPackageService = new ReportPackageService(programService:_programService,
                                                                                  copyOfRecordService:_copyOrRecordService,
                                                                                  timeZoneService:_actualTimeZoneService,
                                                                                  logger:_logger.Object,
                                                                                  linkoExchangeContext:_dbContext,
                                                                                  httpContextService:_httpContext.Object,
                                                                                  userService:_userService,
                                                                                  settingService:_settService,
                                                                                  orgService:_orgService.Object,
                                                                                  sampleService:_sampleService.Object,
                                                                                  mapHelper:_mapHeper,
                                                                                  crommerAuditLogService:_cromerrService.Object,
                                                                                  organizationService:new Mock<IOrganizationService>().Object,
                                                                                  linkoExchangeEmailService:_linkoExchangeEmailService.Object);

            var reportPackageId = 527466233;
            var copyOfRecordDto = reportPackageService.GetCopyOfRecordByReportPackageId(reportPackageId:reportPackageId);

            var certificateDigitalSignatureManager = new CertificateDigitalSignatureManager(dbContext:_dbContext, logger:_logger.Object, httpContextService:_httpContext.Object);

            ////1. Modify signature 
            copyOfRecordDto.Signature = "aaaa";
            var result = certificateDigitalSignatureManager.VerifySignature(currentSignatureStr:copyOfRecordDto.Signature, dataToVerify:copyOfRecordDto.Data, copyOfRecordCertificateId: copyOfRecordDto.CopyOfRecordCertificateId);

            Assert.IsFalse(condition:result);
        }

        [TestMethod]
        public void Verify_wrong_signature_2_return_false()
        {
            var programDto = Mock.Of<OrganizationRegulatoryProgramDto>();

            var programMock = Mock.Get(mocked:_programService);
            programMock.Setup(i => i.GetOrganizationRegulatoryProgram(It.IsAny<int>())).Returns(value:programDto);

            IReportPackageService reportPackageService = new ReportPackageService(programService:_programService,
                                                                                  copyOfRecordService:_copyOrRecordService,
                                                                                  timeZoneService:_actualTimeZoneService,
                                                                                  logger:_logger.Object,
                                                                                  linkoExchangeContext:_dbContext,
                                                                                  httpContextService:_httpContext.Object,
                                                                                  userService:_userService,
                                                                                  settingService:_settService,
                                                                                  orgService:_orgService.Object,
                                                                                  sampleService:_sampleService.Object,
                                                                                  mapHelper:_mapHeper,
                                                                                  crommerAuditLogService:_cromerrService.Object,
                                                                                  organizationService:new Mock<IOrganizationService>().Object,
                                                                                  linkoExchangeEmailService:_linkoExchangeEmailService.Object);

            var reportPackageId = 527466233;
            var copyOfRecordDto = reportPackageService.GetCopyOfRecordByReportPackageId(reportPackageId:reportPackageId);

            var certificateDigitalSignatureManager = new CertificateDigitalSignatureManager(dbContext:_dbContext, logger:_logger.Object, httpContextService:_httpContext.Object);

            ////1. Modify signature  
            var testSingature = "fake signature";
            var testSignatuerBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(s:testSingature));
            copyOfRecordDto.Signature = testSignatuerBase64;
            var result = certificateDigitalSignatureManager.VerifySignature(currentSignatureStr:copyOfRecordDto.Signature, dataToVerify:copyOfRecordDto.Data, copyOfRecordCertificateId: copyOfRecordDto.CopyOfRecordCertificateId);

            Assert.IsFalse(condition:result);
        }

        [TestMethod]
        public void Verify_wrong_data_1_return_false()
        {
            var programDto = Mock.Of<OrganizationRegulatoryProgramDto>();

            var programMock = Mock.Get(mocked:_programService);
            programMock.Setup(i => i.GetOrganizationRegulatoryProgram(It.IsAny<int>())).Returns(value:programDto);

            IReportPackageService reportPackageService = new ReportPackageService(programService:_programService,
                                                                                  copyOfRecordService:_copyOrRecordService,
                                                                                  timeZoneService:_actualTimeZoneService,
                                                                                  logger:_logger.Object,
                                                                                  linkoExchangeContext:_dbContext,
                                                                                  httpContextService:_httpContext.Object,
                                                                                  userService:_userService,
                                                                                  settingService:_settService,
                                                                                  orgService:_orgService.Object,
                                                                                  sampleService:_sampleService.Object,
                                                                                  mapHelper:_mapHeper,
                                                                                  crommerAuditLogService:_cromerrService.Object,
                                                                                  organizationService:new Mock<IOrganizationService>().Object,
                                                                                  linkoExchangeEmailService:_linkoExchangeEmailService.Object);

            var reportPackageId = 527466233;
            var copyOfRecordDto = reportPackageService.GetCopyOfRecordByReportPackageId(reportPackageId:reportPackageId);
            var certificateDigitalSignatureManager = new CertificateDigitalSignatureManager(dbContext:_dbContext, logger:_logger.Object, httpContextService:_httpContext.Object);

            ////1. Fake signature  
            var testSingature = "fake data";
            copyOfRecordDto.Data = Encoding.UTF8.GetBytes(s:testSingature);
            var result = certificateDigitalSignatureManager.VerifySignature(currentSignatureStr:copyOfRecordDto.Signature, dataToVerify:copyOfRecordDto.Data, copyOfRecordCertificateId: copyOfRecordDto.CopyOfRecordCertificateId);

            Assert.IsFalse(condition:result);
        }

        [TestMethod]
        public void Verify_wrong_data_2_return_false()
        {
            var programDto = Mock.Of<OrganizationRegulatoryProgramDto>();

            var programMock = Mock.Get(mocked:_programService);
            programMock.Setup(i => i.GetOrganizationRegulatoryProgram(It.IsAny<int>())).Returns(value:programDto);

            IReportPackageService reportPackageService = new ReportPackageService(programService:_programService,
                                                                                  copyOfRecordService:_copyOrRecordService,
                                                                                  timeZoneService:_actualTimeZoneService,
                                                                                  logger:_logger.Object,
                                                                                  linkoExchangeContext:_dbContext,
                                                                                  httpContextService:_httpContext.Object,
                                                                                  userService:_userService,
                                                                                  settingService:_settService,
                                                                                  orgService:_orgService.Object,
                                                                                  sampleService:_sampleService.Object,
                                                                                  mapHelper:_mapHeper,
                                                                                  crommerAuditLogService:_cromerrService.Object,
                                                                                  organizationService:new Mock<IOrganizationService>().Object,
                                                                                  linkoExchangeEmailService:_linkoExchangeEmailService.Object);

            var reportPackageId = 527466233;
            var copyOfRecordDto = reportPackageService.GetCopyOfRecordByReportPackageId(reportPackageId:reportPackageId);
            var certificateDigitalSignatureManager = new CertificateDigitalSignatureManager(dbContext:_dbContext, logger:_logger.Object, httpContextService:_httpContext.Object);

            ////1. change some of the data  
            copyOfRecordDto.Data[0] = 10;
            var result = certificateDigitalSignatureManager.VerifySignature(currentSignatureStr:copyOfRecordDto.Signature, dataToVerify:copyOfRecordDto.Data, copyOfRecordCertificateId: copyOfRecordDto.CopyOfRecordCertificateId);

            Assert.IsFalse(condition:result);
        }

        [TestMethod]
        public void Verify_empty_data_return_false()
        {
            var programDto = Mock.Of<OrganizationRegulatoryProgramDto>();

            var programMock = Mock.Get(mocked:_programService);
            programMock.Setup(i => i.GetOrganizationRegulatoryProgram(It.IsAny<int>())).Returns(value:programDto);

            IReportPackageService reportPackageService = new ReportPackageService(programService:_programService,
                                                                                  copyOfRecordService:_copyOrRecordService,
                                                                                  timeZoneService:_actualTimeZoneService,
                                                                                  logger:_logger.Object,
                                                                                  linkoExchangeContext:_dbContext,
                                                                                  httpContextService:_httpContext.Object,
                                                                                  userService:_userService,
                                                                                  settingService:_settService,
                                                                                  orgService:_orgService.Object,
                                                                                  sampleService:_sampleService.Object,
                                                                                  mapHelper:_mapHeper,
                                                                                  crommerAuditLogService:_cromerrService.Object,
                                                                                  organizationService:new Mock<IOrganizationService>().Object,
                                                                                  linkoExchangeEmailService:_linkoExchangeEmailService.Object);

            var reportPackageId = 527466233;
            var copyOfRecordDto = reportPackageService.GetCopyOfRecordByReportPackageId(reportPackageId:reportPackageId);
            var certificateDigitalSignatureManager = new CertificateDigitalSignatureManager(dbContext:_dbContext, logger:_logger.Object, httpContextService:_httpContext.Object);

            ////1. assign empty byte array  
            copyOfRecordDto.Data = new byte[] { };
            var result = certificateDigitalSignatureManager.VerifySignature(currentSignatureStr:copyOfRecordDto.Signature, dataToVerify:copyOfRecordDto.Data, copyOfRecordCertificateId: copyOfRecordDto.CopyOfRecordCertificateId);

            Assert.IsFalse(condition:result);
        }

        [TestMethod]
        public void Test_generate_Xml_data()
        {
            //prepare ReportPackageDto 
            var reportPackageDto = GetMockReportPackageDto();

            var programDto = Mock.Of<OrganizationRegulatoryProgramDto>();

            var programMock = Mock.Get(mocked:_programService);
            programMock.Setup(i => i.GetOrganizationRegulatoryProgram(It.IsAny<int>())).Returns(value:programDto);

            IReportPackageService reportPackageService = new ReportPackageService(programService:_programService,
                                                                                  copyOfRecordService:_copyOrRecordService,
                                                                                  timeZoneService:_actualTimeZoneService,
                                                                                  logger:_logger.Object,
                                                                                  linkoExchangeContext:_dbContext,
                                                                                  httpContextService:_httpContext.Object,
                                                                                  userService:_userService,
                                                                                  settingService:_settService,
                                                                                  orgService:_orgService.Object,
                                                                                  sampleService:_sampleService.Object,
                                                                                  mapHelper:_mapHeper,
                                                                                  crommerAuditLogService:_cromerrService.Object,
                                                                                  organizationService:new Mock<IOrganizationService>().Object,
                                                                                  linkoExchangeEmailService:_linkoExchangeEmailService.Object);

            var xmlDate = reportPackageService.GetReportPackageCopyOfRecordDataXmlFile(reportPackageDto:reportPackageDto);
        }

        [TestMethod]
        public void Create_Cor_success_using_mock_reportPackageDto()
        {
            //set 
            var rnd = new Random();
            var rptId = rnd.Next(maxValue:int.MaxValue);

            var reportPackageDto = GetMockReportPackageDto();
            reportPackageDto.ReportPackageId = rptId;
            var copyOfRecordPdfFile = GetCopyOfRecordPdfFile();
            var programDto = Mock.Of<OrganizationRegulatoryProgramDto>();

            var programMock = Mock.Get(mocked:_programService);
            programMock.Setup(i => i.GetOrganizationRegulatoryProgram(It.IsAny<int>()))
                       .Returns(value:programDto);

            IReportPackageService reportPackageService = new ReportPackageService(programService:_programService,
                                                                                  copyOfRecordService:_copyOrRecordService,
                                                                                  timeZoneService:_actualTimeZoneService,
                                                                                  logger:_logger.Object,
                                                                                  linkoExchangeContext:_dbContext,
                                                                                  httpContextService:_httpContext.Object,
                                                                                  userService:_userService,
                                                                                  settingService:_settingService.Object,
                                                                                  orgService:_orgService.Object,
                                                                                  sampleService:_sampleService.Object,
                                                                                  mapHelper:_mapHeper,
                                                                                  crommerAuditLogService:_cromerrService.Object,
                                                                                  organizationService:new Mock<IOrganizationService>().Object,
                                                                                  linkoExchangeEmailService:_linkoExchangeEmailService.Object);

            var xmlFileData = reportPackageService.GetReportPackageCopyOfRecordDataXmlFile(reportPackageDto:reportPackageDto);

            _reprotPackageService.Setup(i => i.GetReportPackageCopyOfRecordDataXmlFile(It.IsAny<ReportPackageDto>())).Returns(value:xmlFileData);

            var copyOfRecordDataXmlFileInfo = _reprotPackageService.Object.GetReportPackageCopyOfRecordDataXmlFile(reportPackageDto:reportPackageDto);

            //var copyOfRecordPdfInfo = _reprotPackageService.Object.GetReportPackageCopyOfRecordPdfFile(reportPackageDto);

            var fileStores = new List<FileStoreDto>();
            foreach (var rpet in reportPackageDto.AttachmentTypes)
            {
                foreach (var fs in rpet.FileStores)
                {
                    fileStores.Add(item:fs);
                }
            }

            _copyOrRecordService.CreateCopyOfRecordForReportPackage(reportPackageId:rptId, attachments:fileStores, copyOfRecordPdfFileDto:copyOfRecordPdfFile,
                                                                    copyOfRecordDataXmlFileDto:copyOfRecordDataXmlFileInfo);
        }

        private ReportPackageDto GetReportPackage(int reportPackageId)
        {
            return new ReportPackageDto
                   {
                       ReportPackageId = reportPackageId,
                       Name = " 1st Quarter PCR",
                       OrganizationRegulatoryProgramId = 3,
                       PeriodStartDateTimeLocal =
                           _actualTimeZoneService
                               .GetLocalizedDateTimeUsingSettingForThisOrg(utcDateTime:DateTime.UtcNow, orgRegProgramId:1), //  DateTimeOffset.UtcNow.LocalDateTime,
                       SubmissionDateTimeOffset =
                           _actualTimeZoneService.GetLocalizedDateTimeUsingSettingForThisOrg(utcDateTime:DateTime.UtcNow, orgRegProgramId:1), // DateTimeOffset.UtcNow,
                       SubmissionDateTimeLocal =
                           _actualTimeZoneService.GetLocalizedDateTimeUsingSettingForThisOrg(utcDateTime:DateTime.UtcNow, orgRegProgramId:1) // DateTimeOffset.UtcNow.LocalDateTime,
                   };
        }

        private CopyOfRecordPdfFileDto GetCopyOfRecordPdfFile()
        {
            var filepath = $"./testFile/CopyOfRecordPdf/CopyOfRecordPdf.pdf";
            var fileDto = new CopyOfRecordPdfFileDto
                          {
                              FileData = File.ReadAllBytes(path:filepath),
                              FileName = Path.GetFileName(path:filepath)
                          };

            return fileDto;
        }

        private CopyOfRecordDataXmlFileDto GetReportPackageCopyOfRecordDataXml()
        {
            var filepath = $"./testFile/CopyOfRecordData/CopyOfRecordData.xml";
            var fileDto = new CopyOfRecordDataXmlFileDto
                          {
                              FileData = File.ReadAllBytes(path:filepath),
                              FileName = Path.GetFileName(path:filepath)
                          };

            return fileDto;
        }

        private ReportPackageDto GetMockReportPackageDto()
        {
            // mock:  submitter userProfileId = 7 
            //        OrgRegProgramUserId = 8 
            //        OrganizationRegulatoryProgramId = 11    

            var orgRegProgamId = 11;
            var orgRegProgram = _dbContext.OrganizationRegulatoryPrograms.Single(i => i.OrganizationRegulatoryProgramId == orgRegProgamId);

            var userProfile = _dbContext.Users.Single(t => t.UserProfileId == orgRegProgamId);
            var recipientOrg = _dbContext.Organizations.Include(path:"Jurisdiction").Single(i => i.OrganizationId == 1000);

            return new ReportPackageDto
                   {
                       ReportPackageId = 15,
                       Name = " 1st Quarter PCR",

                       SubmissionDateTimeLocal = DateTime.Now,
                       SubmissionDateTimeOffset = DateTimeOffset.UtcNow,
                       OrganizationRegulatoryProgramId = 3,
                       OrganizationRegulatoryProgramDto = _mapHeper.GetOrganizationRegulatoryProgramDtoFromOrganizationRegulatoryProgram(orgRegProgram:orgRegProgram),
                       RecipientOrganizationName = recipientOrg.Name,
                       RecipientOrganizationAddressLine1 = recipientOrg.AddressLine1,
                       RecipientOrganizationAddressLine2 = recipientOrg.AddressLine2,
                       RecipientOrganizationCityName = recipientOrg.CityName,
                       RecipientOrganizationJurisdictionName = recipientOrg.Jurisdiction?.Name,
                       RecipientOrganizationZipCode = recipientOrg.ZipCode,
                       SubmitterUserId = userProfile.UserProfileId,
                       SubmitterFirstName = userProfile.FirstName,
                       SubmitterLastName = userProfile.LastName,
                       SubmitterTitleRole = userProfile.TitleRole,
                       SubmitterIPAddress = "::0",
                       SubmitterUserName = userProfile.UserName,

                       PeriodEndDateTime = DateTimeOffset.UtcNow,
                       PeriodStartDateTime = DateTimeOffset.UtcNow,
                       PeriodEndDateTimeLocal = DateTime.UtcNow.ToLocalTime(),
                       PeriodStartDateTimeLocal = DateTime.UtcNow.ToLocalTime(),
                       ReportStatusName = ReportStatusName.ReadyToSubmit,

                       OrganizationName = orgRegProgram.Organization.Name,
                       OrganizationAddressLine1 = orgRegProgram.Organization.AddressLine1,
                       OrganizationAddressLine2 = orgRegProgram.Organization.AddressLine2,
                       OrganizationCityName = orgRegProgram.Organization.CityName,
                       OrganizationJurisdictionName = orgRegProgram.Organization.Jurisdiction?.Name,

                       Comments = "Test comments",
                       OrganizationReferenceNumber = "Test-reference-number",

                       AttachmentTypes = GetMockReportFiles(GetMockAttachmentFiles()),
                       SamplesAndResultsTypes = GetMockReportSamples(GetMockSampleDtos()),
                       CertificationTypes = GetMockCertifications()
                   };
        }

        private List<ReportPackageElementTypeDto> GetMockReportFiles(List<FileStoreDto> mockFileDtos)
        {
            var attachmentTypes = new List<ReportPackageElementTypeDto>();
            var att = new ReportPackageElementTypeDto
                      {
                          ReportElementTypeName = "Lab Analysis Report",
                          FileStores = mockFileDtos
                      };

            attachmentTypes.Add(item:att);
            return attachmentTypes;
        }

        private List<ReportPackageElementTypeDto> GetMockReportSamples(List<SampleDto> mockSampleDtos)
        {
            var samplesResults = new List<ReportPackageElementTypeDto>();
            var rptt = new ReportPackageElementTypeDto
                       {
                           ReportElementTypeName = "Samples and Results",
                           Samples = mockSampleDtos
                       };

            samplesResults.Add(item:rptt);
            return samplesResults;
        }

        private List<SampleDto> GetMockSampleDtos()
        {
            var orgRegProgamId = 11;
            var orgRegProgram = _dbContext.OrganizationRegulatoryPrograms.Include(i => i.Organization.OrganizationType)
                                          .Single(i => i.OrganizationRegulatoryProgramId == orgRegProgamId);

            var sample1 = new SampleDto
                          {
                              SampleId = 1,
                              Name = "Sample 1",
                              MonitoringPointId = 1,
                              MonitoringPointName = "0002-Retired",
                              CtsEventTypeId = 1,
                              CtsEventTypeName = "SNC-P",
                              CtsEventCategoryName = "VIOLATION",
                              CollectionMethodId = 2,
                              CollectionMethodName = "24 hour flow",
                              LabSampleIdentifier = "Test-lab-sample-identifier",
                              StartDateTimeLocal = DateTime.Now,
                              EndDateTimeLocal = DateTime.Now,
                              StartDateTime = DateTimeOffset.UtcNow,
                              EndDateTime = DateTimeOffset.UtcNow,

                              IsReadyToReport = true,
                              FlowUnitValidValues = new[]
                                                    {
                                                        new UnitDto
                                                        {
                                                            UnitId = 1,
                                                            Name = "%"
                                                        },
                                                        new UnitDto
                                                        {
                                                            UnitId = 2,
                                                            Name = "C"
                                                        }
                                                    },

                              ResultQualifierValidValues = "200",
                              MassLoadingCalculationDecimalPlaces = 2,
                              IsMassLoadingResultToUseLessThanSign = true,
                              SampleStatusName = SampleStatusName.ReadyToReport,
                              LastModifierFullName = "Test-Modification-full-name",
                              FlowEnteredValue = "1.11",
                              FlowUnitId = 1,
                              FlowUnitName = "%",
                              SampleResults = new[]
                                              {
                                                  new SampleResultDto
                                                  {
                                                      ConcentrationSampleResultId = 1,
                                                      ParameterId = 1,
                                                      ParameterName = "1,1,1,2-Tetrachloroethane",
                                                      Qualifier = "<",
                                                      EnteredValue = "12.21",
                                                      UnitId = 1,
                                                      UnitName = "%",
                                                      EnteredMethodDetectionLimit = "10",
                                                      MethodDetectionLimit = 9.1,
                                                      AnalysisMethod = "test-analysis method",

                                                      AnalysisDateTimeLocal = DateTime.Now,
                                                      IsApprovedEPAMethod = false,
                                                      LastModifierFullName = "Test-Modification-full-name",
                                                      IsCalcMassLoading = true,
                                                      MassLoadingQualifier = "<",
                                                      MassLoadingValue = "220.0",
                                                      MassLoadingUnitId = 1,
                                                      MassLoadingUnitName = "%"
                                                  },

                                                  new SampleResultDto
                                                  {
                                                      ConcentrationSampleResultId = 2,
                                                      ParameterId = 2,
                                                      ParameterName = "1,1,1-Trichloroethane",
                                                      Qualifier = "<",
                                                      EnteredValue = "4.221",
                                                      UnitId = 1,
                                                      UnitName = "%",
                                                      EnteredMethodDetectionLimit = "333",
                                                      MethodDetectionLimit = 8.5,
                                                      AnalysisMethod = "test-analysis method",

                                                      AnalysisDateTimeLocal = DateTime.Now,
                                                      IsApprovedEPAMethod = false,
                                                      LastModifierFullName = "Test-Modification-full-name",
                                                      IsCalcMassLoading = true,
                                                      MassLoadingQualifier = ">",
                                                      MassLoadingValue = "20.9",
                                                      MassLoadingUnitId = 1,
                                                      MassLoadingUnitName = "F"
                                                  }
                                              },

                              ByOrganizationTypeName = orgRegProgram.Organization.OrganizationType.Name
                          };

            var sample2 = new SampleDto
                          {
                              SampleId = 2,
                              Name = "Sample 2",
                              MonitoringPointId = 2,
                              MonitoringPointName = "427-retired",
                              CtsEventTypeId = 1,
                              CtsEventTypeName = "SNC-P",
                              CtsEventCategoryName = "VIOLATION",
                              CollectionMethodId = 3,
                              CollectionMethodName = "8HR",
                              LabSampleIdentifier = "Test-lab-sample-identifier-2",
                              StartDateTimeLocal = DateTime.Now,
                              EndDateTimeLocal = DateTime.Now,
                              StartDateTime = DateTimeOffset.UtcNow,
                              EndDateTime = DateTimeOffset.UtcNow,

                              IsReadyToReport = true,
                              FlowUnitValidValues = new[]
                                                    {
                                                        new UnitDto
                                                        {
                                                            UnitId = 4,
                                                            Name = "g/day"
                                                        },
                                                        new UnitDto
                                                        {
                                                            UnitId = 5,
                                                            Name = "gpd"
                                                        }
                                                    },

                              ResultQualifierValidValues = "210",
                              MassLoadingCalculationDecimalPlaces = 3,
                              IsMassLoadingResultToUseLessThanSign = true,
                              SampleStatusName = SampleStatusName.ReadyToReport,
                              LastModifierFullName = "Test-Modification-full-name",
                              FlowEnteredValue = "51.23",
                              FlowUnitId = 1,
                              FlowUnitName = "gpd",
                              SampleResults = new[]
                                              {
                                                  new SampleResultDto
                                                  {
                                                      ConcentrationSampleResultId = 3,
                                                      ParameterId = 1,
                                                      ParameterName = "1,1,1,2-Tetrachloroethane",
                                                      Qualifier = "<",
                                                      EnteredValue = "12.21",
                                                      UnitId = 1,
                                                      UnitName = "%",
                                                      EnteredMethodDetectionLimit = "10",
                                                      MethodDetectionLimit = 9.1,
                                                      AnalysisMethod = "test-analysis method",

                                                      AnalysisDateTimeLocal = DateTime.Now,
                                                      IsApprovedEPAMethod = false,
                                                      LastModifierFullName = "Test-Modification-full-name",
                                                      IsCalcMassLoading = true,
                                                      MassLoadingQualifier = "<",
                                                      MassLoadingValue = "220.0",
                                                      MassLoadingUnitId = 1,
                                                      MassLoadingUnitName = "%"
                                                  },

                                                  new SampleResultDto
                                                  {
                                                      ConcentrationSampleResultId = 4,
                                                      ParameterId = 2,
                                                      ParameterName = "1,1,1-Trichloroethane",
                                                      Qualifier = "<",
                                                      EnteredValue = "4.221",
                                                      UnitId = 1,
                                                      UnitName = "%",
                                                      EnteredMethodDetectionLimit = "333",
                                                      MethodDetectionLimit = 8.5,
                                                      AnalysisMethod = "test-analysis method",

                                                      AnalysisDateTimeLocal = DateTime.Now,
                                                      IsApprovedEPAMethod = false,
                                                      LastModifierFullName = "Test-Modification-full-name",
                                                      IsCalcMassLoading = true,
                                                      MassLoadingQualifier = ">",
                                                      MassLoadingValue = "20.9",
                                                      MassLoadingUnitId = 1,
                                                      MassLoadingUnitName = "F"
                                                  }
                                              },

                              ByOrganizationTypeName = orgRegProgram.Organization.OrganizationType.Name
                          };

            var samples = new List<SampleDto> {sample1, sample2};

            return samples;
        }

        private List<FileStoreDto> GetMockAttachmentFiles()
        {
            var fileStoreDtos = new List<FileStoreDto>();
            var fileFolder = $"./testFile/attachments";

            var filesDirectory = new DirectoryInfo(path:fileFolder);
            var files = filesDirectory.EnumerateFiles();
            foreach (var file in files)
            {
                var fileStoreDto = new FileStoreDto
                                   {
                                       Data = File.ReadAllBytes(path:file.FullName),
                                       Name = Path.GetFileName(path:file.Name),
                                       OriginalFileName = file.Name,
                                       ReportElementTypeName = "Lab Analysis Report",
                                       FileType = "Lab Analysis Report",
                                       FileStoreId = 5
                                   };
                fileStoreDtos.Add(item:fileStoreDto);
            }

            return fileStoreDtos;
        }

        private List<ReportPackageElementTypeDto> GetMockCertifications()
        {
            var reportPackageELementTypeDtos = new List<ReportPackageElementTypeDto>();
            var fileFolder = $"./testFile/certifications";

            var filesDirectory = new DirectoryInfo(path:fileFolder);
            var files = filesDirectory.EnumerateFiles();
            foreach (var file in files)
            {
                var rptetdto = new ReportPackageElementTypeDto
                               {
                                   ReportElementTypeContent = File.ReadAllText(path:file.FullName, encoding:Encoding.UTF8),
                                   ReportElementTypeName = Path.GetFileNameWithoutExtension(path:file.Name)
                               };

                reportPackageELementTypeDtos.Add(item:rptetdto);
            }

            return reportPackageELementTypeDtos;
        }
    }
}