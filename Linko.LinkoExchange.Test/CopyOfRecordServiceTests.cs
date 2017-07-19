﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services;
using Linko.LinkoExchange.Services.CopyOfRecord;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.Program;
using Linko.LinkoExchange.Services.Report;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services.TimeZone;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NLog;
using Linko.LinkoExchange.Services.User;
using Linko.LinkoExchange.Services.Email;
using Microsoft.AspNet.Identity;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.QuestionAnswer;
using Linko.LinkoExchange.Services.AuditLog;
using Linko.LinkoExchange.Services.Sample;
using System.Data.Entity;

namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class CopyOfRecordServiceTests
    {
        CopyOfRecordService _copyOrRecordService;

        Mock<IReportPackageService> _reprotPackageService = new Mock<IReportPackageService>();
        readonly Mock<ILogger> _logger = new Mock<ILogger>();
        readonly Mock<IHttpContextService> _httpContext = new Mock<IHttpContextService>();
        readonly ISettingService _settService = Mock.Of<ISettingService>();
        private IProgramService _programService = Mock.Of<IProgramService>();
        private LinkoExchangeContext _dbContext;
        private TimeZoneService _actualTimeZoneService;
        private IUserService _userService;

        private Mock<IAuditLogEntry> _auditLoger = new Mock<IAuditLogEntry>();
        private Mock<IPasswordHasher> _passwordHasher = new Mock<IPasswordHasher>();
        private Mock<IEmailService> emailService = new Mock<IEmailService>();
        private Mock<ISettingService> _settingService = new Mock<ISettingService>();
        private Mock<ISessionCache> _sessionCache = new Mock<ISessionCache>();
        private Mock<IOrganizationService> _orgService = new Mock<IOrganizationService>();
        private Mock<IRequestCache> _requestCache = new Mock<IRequestCache>();
        private Mock<IQuestionAnswerService> _questionAnswerServices = new Mock<IQuestionAnswerService>();
        private MapHelper _mapHeper = new MapHelper();
        private Mock<ICromerrAuditLogService> _crommerAuditLogService = new Mock<ICromerrAuditLogService>();
        private Mock<ISampleService> _sampleService = new Mock<ISampleService>();
        private Mock<ICromerrAuditLogService> _cromerrService = new Mock<ICromerrAuditLogService>();

        [TestInitialize]
        public void Init()
        {
            var connectionString = //ConfigurationManager.ConnectionStrings["LinkoExchangeContext"].ConnectionString;
                  "data source=wtxodev05;initial catalog=LinkoExchange;Integrated Security=True";

            _dbContext = new LinkoExchangeContext(connectionString);

            var settServiceMock = Mock.Get(_settService);
            settServiceMock.Setup(i => i.GetOrganizationSettingValue(It.IsAny<int>(), It.IsAny<SettingType>()))
                .Returns("3");


            _settingService.Setup(i => i.GetOrganizationSettingValue(It.IsAny<int>(), It.IsAny<SettingType>()))
                .Returns("1");

            _actualTimeZoneService = new TimeZoneService(_dbContext, _settService, new MapHelper(), new Mock<IApplicationCache>().Object);
            _httpContext.Setup(s => s.GetClaimValue(It.IsAny<string>())).Returns("1");

            _userService = new UserService(_dbContext, _auditLoger.Object, _passwordHasher.Object, _httpContext.Object, emailService.Object,
                _settingService.Object, _sessionCache.Object, _orgService.Object, _requestCache.Object, _actualTimeZoneService,
                _questionAnswerServices.Object, _logger.Object, _mapHeper, _crommerAuditLogService.Object);


            IDigitalSignatureManager certificateDigitalSignatureManager = new CertificateDigitalSignatureManager(_dbContext, _logger.Object, _httpContext.Object);

            _copyOrRecordService = new CopyOfRecordService
                (_dbContext,
                 _logger.Object,
                 certificateDigitalSignatureManager
                );
        }

        [TestMethod]
        public void Create_Cor_success()
        {
            //set 
            var rnd = new Random();
            int rptId = rnd.Next(int.MaxValue);
            var attachments = GetMockAttachmentFiles();
            var copyOfRecordPdfFile = GetCopyOfRecordPdfFile();
            var reportPackageCopyOfRecordDataXml = GetReportPackageCopyOfRecordDataXml();

            _reprotPackageService.Setup(i => i.GetReportPackageCopyOfRecordDataXmlFile(It.IsAny<ReportPackageDto>()))
                                 .Returns(reportPackageCopyOfRecordDataXml);
            _reprotPackageService.Setup(i => i.GetReportPackageCopyOfRecordPdfFile(It.IsAny<ReportPackageDto>()))
                                 .Returns(copyOfRecordPdfFile);
            var reportPackageDto = new ReportPackageDto();

            var attachmentFiles = attachments;
            var copyOfRecordDataXmlFileInfo = _reprotPackageService.Object.GetReportPackageCopyOfRecordDataXmlFile(reportPackageDto);
            var copyOfRecordPdfInfo = _reprotPackageService.Object.GetReportPackageCopyOfRecordPdfFile(reportPackageDto);

            _copyOrRecordService.CreateCopyOfRecordForReportPackage(rptId, attachmentFiles, copyOfRecordPdfInfo, copyOfRecordDataXmlFileInfo);
        }

        [TestMethod]
        public void Verify_Cor_success()
        {
            var programDto = Mock.Of<OrganizationRegulatoryProgramDto>();

            var programMock = Mock.Get(_programService);
            programMock.Setup(i => i.GetOrganizationRegulatoryProgram(It.IsAny<int>()))
                       .Returns(programDto);

            IReportPackageService reportPackageService =
                new ReportPackageService(_programService,
                                         _copyOrRecordService,
                                         _actualTimeZoneService,
                                         _logger.Object,
                                         _dbContext,
                                         _httpContext.Object,
                                         _userService,
                                         emailService.Object,
                                         _settService,
                                         _orgService.Object,
                                         _sampleService.Object,
                                         _mapHeper,
                                         _cromerrService.Object,
                                         new Mock<IOrganizationService>().Object,
                                         new Mock<IRequestCache>().Object);

            var reportPackageId = 527466233;
            var validResult = reportPackageService.VerififyCopyOfRecord(reportPackageId);

            Assert.IsTrue(validResult.Valid);
        }

        [TestMethod]
        public void Verify_intacted_data_return_true()
        {
            var programDto = Mock.Of<OrganizationRegulatoryProgramDto>();
            var programMock = Mock.Get(_programService);
            programMock.Setup(i => i.GetOrganizationRegulatoryProgram(It.IsAny<int>()))
                       .Returns(programDto);

            IReportPackageService reportPackageService =
                new ReportPackageService(_programService,
                                         _copyOrRecordService,
                                         _actualTimeZoneService,
                                         _logger.Object,
                                         _dbContext,
                                         _httpContext.Object,
                                         _userService,
                                         emailService.Object,
                                         _settService,
                                         _orgService.Object,
                                         _sampleService.Object,
                                         _mapHeper,
                                         _cromerrService.Object,
                                         new Mock<IOrganizationService>().Object,
                                         new Mock<IRequestCache>().Object);

            var reportPackageId = 527466233;
            var copyOfRecordDto = reportPackageService.GetCopyOfRecordByReportPackageId(reportPackageId);
            var certificateDigitalSignatureManager = new CertificateDigitalSignatureManager(_dbContext, _logger.Object, _httpContext.Object);
            var result = certificateDigitalSignatureManager.VerifySignature(copyOfRecordDto.Signature, copyOfRecordDto.Data);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Verify_wrong_signature_1_return_false()
        {
            var programDto = Mock.Of<OrganizationRegulatoryProgramDto>();

            var programMock = Mock.Get(_programService);
            programMock.Setup(i => i.GetOrganizationRegulatoryProgram(It.IsAny<int>()))
                       .Returns(programDto);

            IReportPackageService reportPackageService =
                new ReportPackageService(_programService,
                                         _copyOrRecordService,
                                         _actualTimeZoneService,
                                         _logger.Object,
                                         _dbContext,
                                         _httpContext.Object,
                                         _userService,
                                         emailService.Object,
                                         _settService,
                                         _orgService.Object,
                                         _sampleService.Object,
                                         _mapHeper,
                                         _cromerrService.Object,
                                         new Mock<IOrganizationService>().Object,
                                         new Mock<IRequestCache>().Object);

            var reportPackageId = 527466233;
            var copyOfRecordDto = reportPackageService.GetCopyOfRecordByReportPackageId(reportPackageId);

            var certificateDigitalSignatureManager = new CertificateDigitalSignatureManager(_dbContext, _logger.Object, _httpContext.Object);

            ////1. Modify sigature 
            copyOfRecordDto.Signature = "aaaa";
            var result = certificateDigitalSignatureManager.VerifySignature(copyOfRecordDto.Signature, copyOfRecordDto.Data);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Verify_wrong_signature_2_return_false()
        {
            var programDto = Mock.Of<OrganizationRegulatoryProgramDto>();

            var programMock = Mock.Get(_programService);
            programMock.Setup(i => i.GetOrganizationRegulatoryProgram(It.IsAny<int>()))
                       .Returns(programDto);

            IReportPackageService reportPackageService =
                new ReportPackageService(_programService,
                                         _copyOrRecordService,
                                         _actualTimeZoneService,
                                         _logger.Object,
                                         _dbContext,
                                         _httpContext.Object,
                                         _userService,
                                         emailService.Object,
                                         _settService,
                                         _orgService.Object,
                                         _sampleService.Object,
                                         _mapHeper,
                                         _cromerrService.Object,
                                         new Mock<IOrganizationService>().Object,
                                         new Mock<IRequestCache>().Object);

            var reportPackageId = 527466233;
            var copyOfRecordDto = reportPackageService.GetCopyOfRecordByReportPackageId(reportPackageId);

            var certificateDigitalSignatureManager = new CertificateDigitalSignatureManager(_dbContext, _logger.Object, _httpContext.Object);

            ////1. Modify sigature  
            var testSingature = "fake signature";
            var testSignatuerBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(testSingature));
            copyOfRecordDto.Signature = testSignatuerBase64;
            var result = certificateDigitalSignatureManager.VerifySignature(copyOfRecordDto.Signature, copyOfRecordDto.Data);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Verify_wrong_data_1_return_false()
        {
            var programDto = Mock.Of<OrganizationRegulatoryProgramDto>();

            var programMock = Mock.Get(_programService);
            programMock.Setup(i => i.GetOrganizationRegulatoryProgram(It.IsAny<int>()))
                       .Returns(programDto);

            IReportPackageService reportPackageService =
                new ReportPackageService(_programService, 
                _copyOrRecordService, 
                _actualTimeZoneService,
                  _logger.Object,
                  _dbContext,
                  _httpContext.Object,
                  _userService,
                  emailService.Object,
                  _settService,
                  _orgService.Object,
                  _sampleService.Object,
                  _mapHeper,
                  _cromerrService.Object,
                                         new Mock<IOrganizationService>().Object,
                                         new Mock<IRequestCache>().Object);

            var reportPackageId = 527466233;
            var copyOfRecordDto = reportPackageService.GetCopyOfRecordByReportPackageId(reportPackageId);
            var certificateDigitalSignatureManager = new CertificateDigitalSignatureManager(_dbContext, _logger.Object, _httpContext.Object);

            ////1. Fake signature  
            var testSingature = "fake data";
            copyOfRecordDto.Data = Encoding.UTF8.GetBytes(testSingature);
            var result = certificateDigitalSignatureManager.VerifySignature(copyOfRecordDto.Signature, copyOfRecordDto.Data);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Verify_wrong_data_2_return_false()
        {
            var programDto = Mock.Of<OrganizationRegulatoryProgramDto>();

            var programMock = Mock.Get(_programService);
            programMock.Setup(i => i.GetOrganizationRegulatoryProgram(It.IsAny<int>()))
                       .Returns(programDto);

            IReportPackageService reportPackageService =
                new ReportPackageService(_programService,
                                         _copyOrRecordService,
                                         _actualTimeZoneService,
                                         _logger.Object,
                                         _dbContext,
                                         _httpContext.Object,
                                         _userService,
                                         emailService.Object,
                                         _settService,
                                         _orgService.Object,
                                         _sampleService.Object,
                                         _mapHeper,
                                         _cromerrService.Object,
                                         new Mock<IOrganizationService>().Object,
                                         new Mock<IRequestCache>().Object);

            var reportPackageId = 527466233;
            var copyOfRecordDto = reportPackageService.GetCopyOfRecordByReportPackageId(reportPackageId);
            var certificateDigitalSignatureManager = new CertificateDigitalSignatureManager(_dbContext, _logger.Object, _httpContext.Object);

            ////1. change some of the data  
            copyOfRecordDto.Data[0] = 10;
            var result = certificateDigitalSignatureManager.VerifySignature(copyOfRecordDto.Signature, copyOfRecordDto.Data);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Verify_empty_data_return_false()
        {
            var programDto = Mock.Of<OrganizationRegulatoryProgramDto>();

            var programMock = Mock.Get(_programService);
            programMock.Setup(i => i.GetOrganizationRegulatoryProgram(It.IsAny<int>()))
                       .Returns(programDto);

            IReportPackageService reportPackageService =
                new ReportPackageService(_programService,
                                         _copyOrRecordService,
                                         _actualTimeZoneService,
                                         _logger.Object,
                                         _dbContext,
                                         _httpContext.Object,
                                         _userService,
                                         emailService.Object,
                                         _settService,
                                         _orgService.Object,
                                         _sampleService.Object,
                                         _mapHeper,
                                         _cromerrService.Object,
                                         new Mock<IOrganizationService>().Object,
                                         new Mock<IRequestCache>().Object);

            var reportPackageId = 527466233;
            var copyOfRecordDto = reportPackageService.GetCopyOfRecordByReportPackageId(reportPackageId);
            var certificateDigitalSignatureManager = new CertificateDigitalSignatureManager(_dbContext, _logger.Object, _httpContext.Object);

            ////1. assign empty byte array  
            copyOfRecordDto.Data = new byte[] { };
            var result = certificateDigitalSignatureManager.VerifySignature(copyOfRecordDto.Signature, copyOfRecordDto.Data);

            Assert.IsFalse(result);
        }


        [TestMethod]
        public void Test_generate_Xml_data()
        {
            //prepare ReportPackageDto 
            var reportPackageDto = GetMockReportPackageDto();

            var programDto = Mock.Of<OrganizationRegulatoryProgramDto>();

            var programMock = Mock.Get(_programService);
            programMock.Setup(i => i.GetOrganizationRegulatoryProgram(It.IsAny<int>()))
                       .Returns(programDto);

            IReportPackageService reportPackageService =
                new ReportPackageService(_programService,
                                         _copyOrRecordService,
                                         _actualTimeZoneService,
                                         _logger.Object,
                                         _dbContext,
                                         _httpContext.Object,
                                         _userService,
                                         emailService.Object,
                                         _settService,
                                         _orgService.Object,
                                         _sampleService.Object,
                                         _mapHeper,
                                         _cromerrService.Object,
                                         new Mock<IOrganizationService>().Object,
                                         new Mock<IRequestCache>().Object);

            var xmlDate = reportPackageService.GetReportPackageCopyOfRecordDataXmlFile(reportPackageDto);

        }

        [TestMethod]
        public void Create_Cor_success_using_mock_reportPackageDto()
        {
            //set 
            var rnd = new Random();
            int rptId = rnd.Next(int.MaxValue);

            var reportPackageDto = GetMockReportPackageDto();
            reportPackageDto.ReportPackageId = rptId;
            var copyOfRecordPdfFile = GetCopyOfRecordPdfFile();
            var programDto = Mock.Of<OrganizationRegulatoryProgramDto>();

            var programMock = Mock.Get(_programService);
            programMock.Setup(i => i.GetOrganizationRegulatoryProgram(It.IsAny<int>()))
                       .Returns(programDto);


            IReportPackageService reportPackageService =
                new ReportPackageService(_programService,
                                         _copyOrRecordService,
                                         _actualTimeZoneService,
                                         _logger.Object,
                                         _dbContext,
                                         _httpContext.Object,
                                         _userService,
                                         emailService.Object,
                                         _settingService.Object,
                                         _orgService.Object,
                                         _sampleService.Object,
                                         _mapHeper,
                                         _cromerrService.Object,
                                         new Mock<IOrganizationService>().Object,
                                         new Mock<IRequestCache>().Object);

            var xmlFileData = reportPackageService.GetReportPackageCopyOfRecordDataXmlFile(reportPackageDto);


            _reprotPackageService.Setup(i => i.GetReportPackageCopyOfRecordDataXmlFile(It.IsAny<ReportPackageDto>()))
                                 .Returns(xmlFileData);

            var copyOfRecordDataXmlFileInfo = _reprotPackageService.Object.GetReportPackageCopyOfRecordDataXmlFile(reportPackageDto);
            //var copyOfRecordPdfInfo = _reprotPackageService.Object.GetReportPackageCopyOfRecordPdfFile(reportPackageDto);

            var fileStores = new List<FileStoreDto>();
            foreach (var rpet in reportPackageDto.AttachmentTypes)
            {
                foreach (var fs in rpet.FileStores)
                {
                    fileStores.Add(fs);
                }
            }

            _copyOrRecordService.CreateCopyOfRecordForReportPackage(rptId, fileStores, copyOfRecordPdfFile, copyOfRecordDataXmlFileInfo);
        }

        private ReportPackageDto GetReportPackage(int reportPackageId)
        {
            return new ReportPackageDto
            {
                ReportPackageId = reportPackageId,
                Name = " 1st Quarter PCR",
                OrganizationRegulatoryProgramId = 3,
                PeriodStartDateTimeLocal = _actualTimeZoneService.GetLocalizedDateTimeUsingSettingForThisOrg(DateTime.UtcNow, 1), //  DateTimeOffset.UtcNow.LocalDateTime,
                SubmissionDateTimeOffset = _actualTimeZoneService.GetLocalizedDateTimeUsingSettingForThisOrg(DateTime.UtcNow, 1), // DateTimeOffset.UtcNow,
                SubmissionDateTimeLocal = _actualTimeZoneService.GetLocalizedDateTimeUsingSettingForThisOrg(DateTime.UtcNow, 1), // DateTimeOffset.UtcNow.LocalDateTime,
            };
        }

        private CopyOfRecordPdfFileDto GetCopyOfRecordPdfFile()
        {
            var filepath = $"./testFile/CopyOfRecordPdf/CopyOfRecordPdf.pdf";
            var fileDto = new CopyOfRecordPdfFileDto
            {
                FileData = File.ReadAllBytes(filepath),
                FileName = Path.GetFileName(filepath)
            };

            return fileDto;
        }

        private CopyOfRecordDataXmlFileDto GetReportPackageCopyOfRecordDataXml()
        {
            var filepath = $"./testFile/CopyOfRecordData/CopyOfRecordData.xml";
            var fileDto = new CopyOfRecordDataXmlFileDto
            {
                FileData = File.ReadAllBytes(filepath),
                FileName = Path.GetFileName(filepath)
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
            var recipientOrg = _dbContext.Organizations.Include("Jurisdiction").Single(i => i.OrganizationId == 1000);

            return new ReportPackageDto
            {
                ReportPackageId = 15,
                Name = " 1st Quarter PCR",

                SubmissionDateTimeLocal = DateTime.Now,
                SubmissionDateTimeOffset = DateTimeOffset.UtcNow,
                OrganizationRegulatoryProgramId = 3,
                OrganizationRegulatoryProgramDto = _mapHeper.GetOrganizationRegulatoryProgramDtoFromOrganizationRegulatoryProgram(orgRegProgram),
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
                CertificationTypes = GetMockCertifications(),
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


            attachmentTypes.Add(att);
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

            samplesResults.Add(rptt);
            return samplesResults;
        }

        private List<SampleDto> GetMockSampleDtos()
        {
            var orgRegProgamId = 11;
            var orgRegProgram = _dbContext.OrganizationRegulatoryPrograms
                .Include(i => i.Organization.OrganizationType)
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
                FlowUnitValidValues = new[]{
                    new UnitDto
                    {
                        UnitId = 1,
                        Name = "%"
                        },
                    new UnitDto
                    {
                        UnitId = 2, Name = "C"
                    }
                },

                ResultQualifierValidValues = "200",
                MassLoadingCalculationDecimalPlaces = 2,
                IsMassLoadingResultToUseLessThanSign = true,
                SampleStatusName = SampleStatusName.ReadyToReport,
                LastModifierFullName = "Test-Modification-full-name",
                FlowValue = "1.11",
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
                                         EnteredMethodDetectionLimit ="10",
                                         MethodDetectionLimit = 9.1,
                                         AnalysisMethod = "test-analysis method",

                                         AnalysisDateTimeLocal =  DateTime.Now,
                                         IsApprovedEPAMethod = false,
                                         LastModifierFullName = "Last modificationfull-name",
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
                                         EnteredMethodDetectionLimit ="333",
                                         MethodDetectionLimit = 8.5,
                                         AnalysisMethod = "test-analysis method",

                                         AnalysisDateTimeLocal = DateTime.Now,
                                         IsApprovedEPAMethod = false,
                                         LastModifierFullName = "Last modificationfull-name",
                                         IsCalcMassLoading = true,
                                         MassLoadingQualifier = ">",
                                         MassLoadingValue = "20.9",
                                         MassLoadingUnitId = 1,
                                         MassLoadingUnitName = "F"
                                    },
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
                FlowUnitValidValues = new[]{
                    new UnitDto
                    {
                        UnitId = 4,
                        Name = "g/day"
                        },
                    new UnitDto
                    {
                        UnitId = 5, Name = "gpd"
                    }
                },

                ResultQualifierValidValues = "210",
                MassLoadingCalculationDecimalPlaces = 3,
                IsMassLoadingResultToUseLessThanSign = true,
                SampleStatusName = SampleStatusName.ReadyToReport,
                LastModifierFullName = "Test-Modification-full-name",
                FlowValue = "51.23",
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
                                         EnteredMethodDetectionLimit ="10",
                                         MethodDetectionLimit = 9.1,
                                         AnalysisMethod = "test-analysis method",

                                         AnalysisDateTimeLocal = DateTime.Now,
                                         IsApprovedEPAMethod = false,
                                         LastModifierFullName = "Last modificationfull-name",
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
                                         EnteredMethodDetectionLimit ="333",
                                         MethodDetectionLimit = 8.5,
                                         AnalysisMethod = "test-analysis method",

                                         AnalysisDateTimeLocal = DateTime.Now,
                                         IsApprovedEPAMethod = false,
                                         LastModifierFullName = "Last modificationfull-name",
                                         IsCalcMassLoading = true,
                                         MassLoadingQualifier = ">",
                                         MassLoadingValue = "20.9",
                                         MassLoadingUnitId = 1,
                                         MassLoadingUnitName = "F"
                                    },
                                },

                ByOrganizationTypeName = orgRegProgram.Organization.OrganizationType.Name
            };

            var samples = new List<SampleDto>();
            samples.Add(sample1);
            samples.Add(sample2);

            return samples;
        }

        private List<FileStoreDto> GetMockAttachmentFiles()
        {
            var fileStoreDtos = new List<FileStoreDto>();
            var fileFolder = $"./testFile/attachments";

            var filesDirectory = new DirectoryInfo(fileFolder);
            var files = filesDirectory.EnumerateFiles();
            foreach (var file in files)
            {
                var fileStoreDto = new FileStoreDto
                {
                    Data = File.ReadAllBytes(file.FullName),
                    Name = Path.GetFileName(file.Name),
                    OriginalFileName = file.Name,
                    ReportElementTypeName = "Lab Analysis Report",
                    FileType = "Lab Analysis Report",
                    FileStoreId = 5
                };
                fileStoreDtos.Add(fileStoreDto);
            }

            return fileStoreDtos;
        }

        private List<ReportPackageElementTypeDto> GetMockCertifications()
        {
            var reportPackageELementTypeDtos = new List<ReportPackageElementTypeDto>();
            var fileFolder = $"./testFile/certifications";

            var filesDirectory = new DirectoryInfo(fileFolder);
            var files = filesDirectory.EnumerateFiles();
            foreach (var file in files)
            {
                var rptetdto = new ReportPackageElementTypeDto
                {
                    ReportElementTypeContent = File.ReadAllText(file.FullName, Encoding.UTF8),
                    ReportElementTypeName = Path.GetFileNameWithoutExtension(file.Name)
                };

                reportPackageELementTypeDtos.Add(rptetdto);
            }

            return reportPackageELementTypeDtos;
        }
    }
}
