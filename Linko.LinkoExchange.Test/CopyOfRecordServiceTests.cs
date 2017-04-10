using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services;
using Linko.LinkoExchange.Services.CopyOfRecord;
using Linko.LinkoExchange.Services.CopyOrRecord;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.Organization;
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

        [TestInitialize]
        public void Init()
        {
            var connectionString = //ConfigurationManager.ConnectionStrings["LinkoExchangeContext"].ConnectionString;
                  "data source=wtxodev05;initial catalog=LinkoExchange;Integrated Security=True";
            var dbContext = new LinkoExchangeContext(connectionString);

            var users = dbContext.Users.Where(i => i.IsAccountLocked == false).ToList();

            var actualTimeZoneService = new TimeZoneService(dbContext, _settService, new MapHelper());
            _httpContext.Setup(s => s.GetClaimValue(It.IsAny<string>())).Returns("1");

            IDigitalSignatureManager certificateDigitalSignatureManager = new CertificateDigitalSignatureManager(dbContext, new MapHelper(), _logger.Object, _httpContext.Object);

            _copyOrRecordService = new CopyOfRecordService
                (dbContext,
                 new MapHelper(),
                 _logger.Object,
                 _httpContext.Object,
                 actualTimeZoneService,
                 _reprotPackageService.Object,
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

            var attachments = GetMockAttachmentFiles();
            var certifications = GetMockCertifications();
            var sampleResults = GetCorPreviewFile();
            var manifest = GetReportPackageManefestData();

            _reprotPackageService.Setup(i => i.GetReportPackageAttachments(It.IsAny<int>()))
                                 .Returns(attachments);

            _reprotPackageService.Setup(i => i.GetReportPackageCertifications(It.IsAny<int>()))
                                 .Returns(certifications);

            _reprotPackageService.Setup(i => i.GetReportPackageManifestData(It.IsAny<int>()))
                                 .Returns(manifest);
            _reprotPackageService.Setup(i => i.GetReportPackageSampleFormData(It.IsAny<int>()))
                                 .Returns(sampleResults);

            var rnd = new Random();
            int rptId = rnd.Next(int.MaxValue);

            _copyOrRecordService.CreateCopyOfRecordForReportPackage(rptId);
        }


        private CorPreviewFileDto GetCorPreviewFile()
        {
            var filepath = $"./testFile/preview/linko-cor-preview.pdf";
            var fileDto = new CorPreviewFileDto
            {
                FileData = File.ReadAllBytes(filepath),
                FileName = Path.GetFileName(filepath)
            };

            return fileDto;
        }

        private CorManifestFileDato GetReportPackageManefestData()
        {
            var filepath = $"./testFile/manifest/manifest.xml";
            var fileDto = new CorManifestFileDato
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

        public class CertificateDigitalSignatureManagerMock : CertificateDigitalSignatureManager
        {
            public CertificateDigitalSignatureManagerMock(LinkoExchangeContext dbContext, IMapHelper mapHelper, ILogger logger, IHttpContextService httpContextService)
                : base(dbContext, mapHelper, logger, httpContextService)
            {
            }
        }

    }
}
