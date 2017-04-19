using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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


        [TestInitialize]
        public void Init()
        {
            var connectionString = //ConfigurationManager.ConnectionStrings["LinkoExchangeContext"].ConnectionString;
                  "data source=wtxodev05;initial catalog=LinkoExchange;Integrated Security=True";

            _dbContext = new LinkoExchangeContext(connectionString);
            _actualTimeZoneService = new TimeZoneService(_dbContext, _settService, new MapHelper());
            _httpContext.Setup(s => s.GetClaimValue(It.IsAny<string>())).Returns("1");


            _userService = new UserService(_dbContext, _auditLoger.Object, _passwordHasher.Object, _httpContext.Object, emailService.Object,
                _settingService.Object, _sessionCache.Object, _orgService.Object, _requestCache.Object, _actualTimeZoneService,
                _questionAnswerServices.Object, _logger.Object, _mapHeper, _crommerAuditLogService.Object);


            IDigitalSignatureManager certificateDigitalSignatureManager = new CertificateDigitalSignatureManager(_dbContext, new MapHelper(), _logger.Object, _httpContext.Object);

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

            _reprotPackageService.Setup(i => i.GetReportPackageCopyOfRecordDataXmlFile(It.IsAny<int>()))
                                 .Returns(reportPackageCopyOfRecordDataXml);
            _reprotPackageService.Setup(i => i.GetReportPackageCopyOfRecordPdfFile(It.IsAny<int>()))
                                 .Returns(copyOfRecordPdfFile);

            var attachmentFiles = attachments;
            var copyOfRecordDataXmlFileInfo = _reprotPackageService.Object.GetReportPackageCopyOfRecordDataXmlFile(rptId);
            var copyOfRecordPdfInfo = _reprotPackageService.Object.GetReportPackageCopyOfRecordPdfFile(rptId);

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
                new ReportPackageService(_programService, _copyOrRecordService, _actualTimeZoneService,
                  _logger.Object,
                  _dbContext,
                  _httpContext.Object,
                  _userService,
                 emailService.Object,
                 _settService,
                 _mapHeper);

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
                new ReportPackageService(_programService, _copyOrRecordService, _actualTimeZoneService,
                  _logger.Object,
                  _dbContext,
                  _httpContext.Object,
                  _userService,
                 emailService.Object,
                  _settService, _mapHeper);

            var reportPackageId = 527466233;
            var copyOfRecordDto = reportPackageService.GetCopyOfRecordByReportPackageId(reportPackageId);
            var certificateDigitalSignatureManager = new CertificateDigitalSignatureManager(_dbContext, new MapHelper(), _logger.Object, _httpContext.Object);
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
                new ReportPackageService(_programService, _copyOrRecordService, _actualTimeZoneService,
                  _logger.Object,
                  _dbContext,
                  _httpContext.Object,
                  _userService,
                 emailService.Object,
                  _settService, _mapHeper);

            var reportPackageId = 527466233;
            var copyOfRecordDto = reportPackageService.GetCopyOfRecordByReportPackageId(reportPackageId);

            var certificateDigitalSignatureManager = new CertificateDigitalSignatureManager(_dbContext, new MapHelper(), _logger.Object, _httpContext.Object);

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
                new ReportPackageService(_programService, _copyOrRecordService, _actualTimeZoneService,
                  _logger.Object,
                  _dbContext,
                  _httpContext.Object,
                  _userService,
                 emailService.Object,
                  _settService, _mapHeper);

            var reportPackageId = 527466233;
            var copyOfRecordDto = reportPackageService.GetCopyOfRecordByReportPackageId(reportPackageId);

            var certificateDigitalSignatureManager = new CertificateDigitalSignatureManager(_dbContext, new MapHelper(), _logger.Object, _httpContext.Object);

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
                new ReportPackageService(_programService, _copyOrRecordService, _actualTimeZoneService,
                  _logger.Object,
                  _dbContext,
                  _httpContext.Object,
                  _userService,
                 emailService.Object,
                  _settService, _mapHeper);

            var reportPackageId = 527466233;
            var copyOfRecordDto = reportPackageService.GetCopyOfRecordByReportPackageId(reportPackageId);
            var certificateDigitalSignatureManager = new CertificateDigitalSignatureManager(_dbContext, new MapHelper(), _logger.Object, _httpContext.Object);

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
                new ReportPackageService(_programService, _copyOrRecordService, _actualTimeZoneService,
                  _logger.Object,
                  _dbContext,
                  _httpContext.Object,
                  _userService,
                 emailService.Object,
                  _settService, _mapHeper);

            var reportPackageId = 527466233;
            var copyOfRecordDto = reportPackageService.GetCopyOfRecordByReportPackageId(reportPackageId);
            var certificateDigitalSignatureManager = new CertificateDigitalSignatureManager(_dbContext, new MapHelper(), _logger.Object, _httpContext.Object);

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
                new ReportPackageService(_programService, _copyOrRecordService, _actualTimeZoneService,
                  _logger.Object,
                  _dbContext,
                  _httpContext.Object,
                  _userService,
                 emailService.Object,
                  _settService, _mapHeper);

            var reportPackageId = 527466233;
            var copyOfRecordDto = reportPackageService.GetCopyOfRecordByReportPackageId(reportPackageId);
            var certificateDigitalSignatureManager = new CertificateDigitalSignatureManager(_dbContext, new MapHelper(), _logger.Object, _httpContext.Object);

            ////1. assign empty byte array  
            copyOfRecordDto.Data = new byte[] { };
            var result = certificateDigitalSignatureManager.VerifySignature(copyOfRecordDto.Signature, copyOfRecordDto.Data);

            Assert.IsFalse(result);
        }

        private ReportPackageDto GetReportPackage(int reportPackageId)
        {
            return new ReportPackageDto
            {
                ReportPackageId = reportPackageId,
                Name = " 1st Quarter PCR",
                OrganizationRegulatoryProgramId = 3,
                SubMissionDateTimeLocal = DateTime.UtcNow
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
                    Name = Path.GetFileName(file.Name)
                };
                fileStoreDtos.Add(fileStoreDto);
            }

            return fileStoreDtos;
        }

        private List<ReportPackageELementTypeDto> GetMockCertifications()
        {
            var reportPackageELementTypeDtos = new List<ReportPackageELementTypeDto>();
            var fileFolder = $"./testFile/certifications";

            var filesDirectory = new DirectoryInfo(fileFolder);
            var files = filesDirectory.EnumerateFiles();
            foreach (var file in files)
            {
                var rptetdto = new ReportPackageELementTypeDto
                {
                    ReportElementTypeContent = File.ReadAllText(file.FullName),
                    ReportElementTypeName = Path.GetFileNameWithoutExtension(file.Name)
                };

                reportPackageELementTypeDtos.Add(rptetdto);
            }

            return reportPackageELementTypeDtos;
        }
    }
}
