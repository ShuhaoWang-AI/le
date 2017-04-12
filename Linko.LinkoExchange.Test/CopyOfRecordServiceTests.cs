using System;
using System.Collections.Generic;
using System.IO;
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
namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class CopyOfRecordServiceTests
    {
        CopyOfRecordService _copyOrRecordService;

        Mock<IReportPackageService> _reprotPackageService = new Mock<IReportPackageService>();
        Mock<ILogger> _logger = new Mock<ILogger>();
        Mock<IHttpContextService> _httpContext = new Mock<IHttpContextService>();
        ISettingService _settService = Mock.Of<ISettingService>();
        private IProgramService _programService = Mock.Of<IProgramService>();

        [TestInitialize]
        public void Init()
        {
            var connectionString = //ConfigurationManager.ConnectionStrings["LinkoExchangeContext"].ConnectionString;
                  "data source=wtxodev05;initial catalog=LinkoExchange;Integrated Security=True";
            var dbContext = new LinkoExchangeContext(connectionString);

            var actualTimeZoneService = new TimeZoneService(dbContext, _settService, new MapHelper());
            _httpContext.Setup(s => s.GetClaimValue(It.IsAny<string>())).Returns("1");

            IDigitalSignatureManager certificateDigitalSignatureManager = new CertificateDigitalSignatureManager(dbContext, new MapHelper(), _logger.Object, _httpContext.Object);

            _copyOrRecordService = new CopyOfRecordService
                (dbContext,
                 new MapHelper(),
                 _logger.Object,
                 _httpContext.Object,
                 actualTimeZoneService,
                 certificateDigitalSignatureManager
                );
        }

        [TestMethod]
        public void Create_Cor_success()
        {
            //set 
            var fileDtos = new List<FileStoreDto>();
            var ft1 = new FileStoreDto { Name = "attachment_1.pdf" };
            var ft2 = new FileStoreDto { Name = "attachment_2.pdf" };

            fileDtos.AddRange(collection: new[] { ft1, ft2 });

            var rnd = new Random();
            int rptId = rnd.Next(int.MaxValue);

            var attachments = GetMockAttachmentFiles();
            var copyOfRecordPdfFile = GetCopyOfRecordPdfFile();
            var reportPackageCopyOfRecordDataXml = GetReportPackageCopyOfRecordDataXml();

            _reprotPackageService.Setup(i => i.GetReportPackageAttachments(It.IsAny<int>()))
                                 .Returns(attachments);

            _reprotPackageService.Setup(i => i.GetReportPackageCopyOfRecordDataXmlFile(It.IsAny<int>()))
                                 .Returns(reportPackageCopyOfRecordDataXml);
            _reprotPackageService.Setup(i => i.GetReportPackageCopyOfRecordPdfFile(It.IsAny<int>()))
                                 .Returns(copyOfRecordPdfFile);

            var attachmentFiles = _reprotPackageService.Object.GetReportPackageAttachments(rptId);
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

            IReportPackageService reportPackageService = new ReportPackageService(_programService, _copyOrRecordService);

            var reportPackageId = 527466233;
            var verified = reportPackageService.VerififyCopyOfRecord(reportPackageId);

            Assert.IsTrue(verified);
        }

        private ReportPackageDto GetReportPackage(int reportPackageId)
        {
            return new ReportPackageDto
            {
                ReportPackageId = reportPackageId,
                Name = " 1st Quarter PCR",
                OrganizationRegulatoryProgramId = 3,
                SubMissionDateTime = DateTime.UtcNow
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

                //fileStoreDto.Data = new byte[data.Length];
                //Array.Copy(data, fileStoreDto.Data, data.Length); 
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
