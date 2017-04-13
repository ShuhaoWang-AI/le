using System.Collections.Generic;
using System.Configuration;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NLog;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services;
using Linko.LinkoExchange.Services.Organization;
using System;
using Linko.LinkoExchange.Services.Parameter;
using Linko.LinkoExchange.Services.TimeZone;
using Linko.LinkoExchange.Services.Sample;

namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class SampleServiceTests
    {
        SampleService _sampleService;
        Mock<IHttpContextService> _httpContext;
        Mock<IOrganizationService> _orgService;
        Mock<ILogger> _logger;
        Mock<ITimeZoneService> _timeZoneService;
        Mock<ISettingService> _settingsService;

        public SampleServiceTests()
        {
        }

        [TestInitialize]
        public void Initialize()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["LinkoExchangeContext"].ConnectionString;
            var connection = new LinkoExchangeContext(connectionString);
            _httpContext = new Mock<IHttpContextService>();
            _orgService = new Mock<IOrganizationService>();
            _logger = new Mock<ILogger>();
            _timeZoneService = new Mock<ITimeZoneService>();
            _settingsService = new Mock<ISettingService>();

            var actualTimeZoneService = new TimeZoneService(connection, new SettingService(connection, _logger.Object, new MapHelper()), new MapHelper());
            var actualSettings = new SettingService(connection, _logger.Object, new MapHelper());

            _httpContext.Setup(s => s.GetClaimValue(It.IsAny<string>())).Returns("1");
            _orgService.Setup(s => s.GetAuthority(It.IsAny<int>())).Returns(new OrganizationRegulatoryProgramDto() { OrganizationRegulatoryProgramId = 1 });

            _sampleService = new SampleService(connection, 
                _httpContext.Object, 
                _orgService.Object, 
                new MapHelper(), 
                _logger.Object,
                actualTimeZoneService,
                actualSettings);
        }


        [TestMethod]
        public void SaveSample_DetailsOnly_NoResults_Valid()
        {
            var sampleDto = new SampleDto();
            _sampleService.SaveSample(sampleDto);
        }

        private SampleDto GetTestSampleDto()
        {
            var sampleDto = new SampleDto();
            sampleDto.Name = "Sample XYZ";
            sampleDto.MonitoringPointId = 1;
            sampleDto.MonitoringPointName = "0002-Retired";
            sampleDto.CollectionMethodId = 1;
            sampleDto.CollectionMethodName = "24";
            sampleDto.CtsEventTypeId = 1;
            sampleDto.CtsEventTypeName = "SNC-P";
            sampleDto.CtsEventCategoryName = "Sample Category 1";
            //sampleDto.OrganizationTypeId = 1;
            //sampleDto.OrganizationRegulatoryProgramId = 1;
            sampleDto.FlowUnitId = 10;
            sampleDto.FlowUnitName = "ppd";
            sampleDto.FlowValue = 808.1;
            sampleDto.StartDateTimeLocal = DateTime.Now;
            sampleDto.EndDateTimeLocal = DateTime.Now;
            sampleDto.IsReadyToReport = true;
            sampleDto.IsCalculated = false;
            var resultDtos = new List<SampleResultDto>();

            var resultDto = new SampleResultDto()
            {
                ParameterId = 14,
                ParameterName = "1,2-Dibromo-3-chloropropane",
                Qualifier = ">",
                UnitId = 7,
                UnitName = "mg/L",
                Value = 20,
                DecimalPlaces = 4,
                MethodDetectionLimit = "MDL 2",
                AnalysisMethod = "Analysis Method 2",
                AnalysisDateTimeLocal = DateTime.Now,
                IsApprovedEPAMethod = true,
                IsCalcMassLoading = true,
                MassLoadingQualifier = ">",
                MassLoadingUnitId = 8,
                MassLoadingUnitName = "mgd",
                MassLoadingValue = 1001.2005,
                MassLoadingDecimalPlaces = 4
            };
            resultDtos.Add(resultDto);
            resultDto = new SampleResultDto()
            {
                ParameterId = 263,
                ParameterName = "Trichloroethane",
                Qualifier = "<",
                UnitId = 11,
                UnitName = "su",
                Value = 991,
                DecimalPlaces = 2,
                MethodDetectionLimit = "MDL 5",
                AnalysisMethod = "Analysis Method 5",
                AnalysisDateTimeLocal = DateTime.Now,
                IsApprovedEPAMethod = false,
                IsCalcMassLoading = false,
            };
            resultDtos.Add(resultDto);

            sampleDto.SampleResults = resultDtos;

            return sampleDto;
        }

        [TestMethod]
        public void SaveSample_Details_Concentration_And_Mass_Results_Valid_NotSavingAsReadyToReport()
        {
            var sampleDto = GetTestSampleDto();
            sampleDto.IsReadyToReport = false;
            _sampleService.SaveSample(sampleDto, false);
        }

        [TestMethod]
        public void SaveSample_Details_Concentration_And_Mass_Results_Valid_SavingAsReadyToReport()
        {
            var sampleDto = GetTestSampleDto();
            sampleDto.IsReadyToReport = true;
            _sampleService.SaveSample(sampleDto, true);
        }

        [TestMethod]
        public void GetSampleDetails_With_Results_Valid()
        {
            var sampleId = -1;
            var sampleDtos = _sampleService.GetSamples(SampleStatusName.All);
            foreach (var sampleDto in sampleDtos)
            {
                sampleId = sampleDto.SampleId.Value;
                break;
            }

            var firstSampleDto = _sampleService.GetSampleDetails(sampleId);
        }

        [TestMethod]
        public void Delete_Sample_With_Results_Valid()
        {
            var sampleId = -1;
            var sampleDtos = _sampleService.GetSamples(SampleStatusName.All);
            foreach (var sampleDto in sampleDtos)
            {
                sampleId = sampleDto.SampleId.Value;
                break;
            }

            _sampleService.DeleteSample(sampleId);
        }

        [TestMethod]
        public void Get_Samples_And_Test_Validity_DRAFT()
        {
            var sampleDtos = _sampleService.GetSamples(SampleStatusName.All);
            foreach (var sampleDto in sampleDtos)
            {
                var isValid = _sampleService.IsValidSample(sampleDto, false, false);
            }
        }

        [TestMethod]
        public void Get_Samples_And_Test_Validity_READYTOSUBMIT()
        {
            var sampleDtos = _sampleService.GetSamples(SampleStatusName.All);
            foreach (var sampleDto in sampleDtos)
            {
                var isValid = _sampleService.IsValidSample(sampleDto, true, false);
            }
        }

        [TestMethod]
        public void GetSamples_Only_Reported_Status()
        {
            var sampleDtos = _sampleService.GetSamples(SampleStatusName.Reported);
            
        }
    }
}
