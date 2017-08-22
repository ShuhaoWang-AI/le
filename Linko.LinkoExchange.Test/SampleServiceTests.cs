using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Resources;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.HttpContext;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Sample;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services.TimeZone;
using Linko.LinkoExchange.Services.Unit;
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
    public class SampleServiceTests
    {
        #region fields

        private Mock<IHttpContextService> _httpContext;
        private Mock<ILogger> _logger;
        private Mock<IOrganizationService> _orgService;
        private SampleService _sampleService;
        private Mock<ISettingService> _settingsService;
        private Mock<ITimeZoneService> _timeZoneService;
        private Mock<IUnitService> _unitService;

        #endregion

        [TestInitialize]
        public void Initialize()
        {
            var connectionString = ConfigurationManager.ConnectionStrings[name:"LinkoExchangeContext"].ConnectionString;
            var connection = new LinkoExchangeContext(nameOrConnectionString:connectionString);
            _httpContext = new Mock<IHttpContextService>();
            _orgService = new Mock<IOrganizationService>();
            _logger = new Mock<ILogger>();
            _timeZoneService = new Mock<ITimeZoneService>();
            _settingsService = new Mock<ISettingService>();
            _unitService = new Mock<IUnitService>();

            var actualTimeZoneService = new TimeZoneService(dbContext:connection,
                                                            settings:new SettingService(dbContext:connection, logger:_logger.Object, mapHelper:new MapHelper(),
                                                                                        cache:new Mock<IRequestCache>().Object, globalSettings:new Mock<IGlobalSettings>().Object),
                                                            mapHelper:new MapHelper(), appCache:new Mock<IApplicationCache>().Object);
            var actualSettings = new SettingService(dbContext:connection, logger:_logger.Object, mapHelper:new MapHelper(), cache:new Mock<IRequestCache>().Object,
                                                    globalSettings:new Mock<IGlobalSettings>().Object);

            _httpContext.Setup(s => s.GetClaimValue(It.IsAny<string>())).Returns(value:"1");
            _orgService.Setup(s => s.GetAuthority(It.IsAny<int>()))
                       .Returns(value:new OrganizationRegulatoryProgramDto {OrganizationRegulatoryProgramId = 1, OrganizationId = 1000});

            var actualUnitService = new UnitService(dbContext:connection,
                                                    mapHelper:new MapHelper(),
                                                    logger:_logger.Object,
                                                    httpContextService:_httpContext.Object,
                                                    timeZoneService:actualTimeZoneService,
                                                    orgService:_orgService.Object,
                                                    settingService:actualSettings,
                                                    requestCache:new Mock<IRequestCache>().Object);

            _sampleService = new SampleService(dbContext:connection,
                                               httpContext:_httpContext.Object,
                                               orgService:_orgService.Object,
                                               mapHelper:new MapHelper(),
                                               logger:_logger.Object,
                                               timeZoneService:actualTimeZoneService,
                                               settings:actualSettings,
                                               unitService:actualUnitService,
                                               cache:new Mock<IApplicationCache>().Object);
        }

        [TestMethod]
        public void SaveSample()
        {
            //Create test Sample Dto
            var sampleDto = GetTestSampleDto();
            sampleDto.IsMassLoadingResultToUseLessThanSign = true;
            sampleDto.IsReadyToReport = true;

            //Persist
            try
            {
                var id = _sampleService.SaveSample(sampleDto:sampleDto);
                Assert.IsTrue(condition:id > 0, message:"Sample didn't save");
            }
            catch
            {
                // ignored
            }
        }

        [TestMethod]
        public void SaveSample_Invalid_Sample_Type()
        {
            var sampleDto = new SampleDto();
            try
            {
                _sampleService.SaveSample(sampleDto:sampleDto);
            }
            catch (RuleViolationException rve)
            {
                Assert.AreEqual(expected:"Sample Type is required.", actual:rve.ValidationIssues[index:0].ErrorMessage);
            }
        }

        [TestMethod]
        public void SaveSample_Invalid_Collection_Method()
        {
            var sampleDto = new SampleDto();
            sampleDto.CtsEventTypeId = 1;
            try
            {
                _sampleService.SaveSample(sampleDto:sampleDto);
            }
            catch (RuleViolationException rve)
            {
                Assert.AreEqual(expected:"Collection Method is required.", actual:rve.ValidationIssues[index:0].ErrorMessage);
            }
        }

        [TestMethod]
        public void SaveSample_Invalid_Start_Date()
        {
            var sampleDto = new SampleDto();
            sampleDto.CtsEventTypeId = 1;
            sampleDto.CollectionMethodId = 1;
            try
            {
                _sampleService.SaveSample(sampleDto:sampleDto);
            }
            catch (RuleViolationException rve)
            {
                Assert.AreEqual(expected:"Start Date/Time is required.", actual:rve.ValidationIssues[index:0].ErrorMessage);
            }
        }

        [TestMethod]
        public void SaveSample_Invalid_End_Date()
        {
            var sampleDto = new SampleDto();
            sampleDto.CtsEventTypeId = 1;
            sampleDto.CollectionMethodId = 1;
            sampleDto.StartDateTimeLocal = DateTime.Now;
            try
            {
                _sampleService.SaveSample(sampleDto:sampleDto);
            }
            catch (RuleViolationException rve)
            {
                Assert.AreEqual(expected:"End Date/Time is required.", actual:rve.ValidationIssues[index:0].ErrorMessage);
            }
        }

        [TestMethod]
        public void SaveSample_Future_Start_Date()
        {
            var sampleDto = new SampleDto();
            sampleDto.CtsEventTypeId = 1;
            sampleDto.CollectionMethodId = 1;
            sampleDto.StartDateTimeLocal = DateTime.SpecifyKind(value:DateTime.Now.AddDays(value:1), kind:DateTimeKind.Unspecified);
            sampleDto.EndDateTimeLocal = DateTime.SpecifyKind(value:DateTime.Now.AddDays(value:-1), kind:DateTimeKind.Unspecified);
            try
            {
                _sampleService.SaveSample(sampleDto:sampleDto);
            }
            catch (RuleViolationException rve)
            {
                Assert.AreEqual(expected:"Sample dates cannot be future dates.", actual:rve.ValidationIssues[index:0].ErrorMessage);
            }
        }

        [TestMethod]
        public void SaveSample_Future_End_Date()
        {
            var sampleDto = new SampleDto();
            sampleDto.CtsEventTypeId = 1;
            sampleDto.CollectionMethodId = 1;
            sampleDto.StartDateTimeLocal = DateTime.Now.AddDays(value:-1);
            sampleDto.EndDateTimeLocal = DateTime.Now.AddDays(value:1);
            try
            {
                _sampleService.SaveSample(sampleDto:sampleDto);
            }
            catch (RuleViolationException rve)
            {
                Assert.AreEqual(expected:"Sample dates cannot be future dates.", actual:rve.ValidationIssues[index:0].ErrorMessage);
            }
        }

        [TestMethod]
        public void SaveSample_Details_Concentration_And_Mass_Results_Valid_NotSavingAsReadyToReport()
        {
            var sampleDto = GetTestSampleDto();
            sampleDto.IsReadyToReport = false;
            _sampleService.SaveSample(sampleDto:sampleDto);
        }

        [TestMethod]
        public void SaveSample_ReadyToReport_Mass_Results_Missing_Flow_Unit()
        {
            var sampleDto = GetTestSampleDto();
            sampleDto.IsReadyToReport = true;
            try
            {
                _sampleService.SaveSample(sampleDto:sampleDto);
            }
            catch (RuleViolationException rve)
            {
                Assert.AreEqual(expected:"You must provide valid a flow value to calculate mass loading results.", actual:rve.ValidationIssues[index:0].ErrorMessage);
            }
        }

        [TestMethod]
        public void SaveSample_Flow_Unit_Provided_But_Missing_Flow_Value()
        {
            var sampleDto = GetTestSampleDto();
            sampleDto.FlowUnitId = 1;
            sampleDto.FlowEnteredValue = null;
            sampleDto.IsReadyToReport = false;
            try
            {
                _sampleService.SaveSample(sampleDto:sampleDto);
            }
            catch (RuleViolationException rve)
            {
                Assert.AreEqual(expected:"Flow value and flow unit must be provided together.", actual:rve.ValidationIssues[index:0].ErrorMessage);
            }
        }

        [TestMethod]
        public void SaveSample_ReadyToReport_NonNumeric_Qualifier_With_Value()
        {
            var sampleDto = GetTestSampleDto();
            var sampleResults = new List<SampleResultDto>();
            var invalidResultDto = new SampleResultDto
                                   {
                                       Qualifier = "ND",
                                       EnteredValue = "99",
                                       UnitId = 1
                                   };
            sampleResults.Add(item:invalidResultDto);
            sampleDto.SampleResults = sampleResults;
            sampleDto.IsReadyToReport = true;
            try
            {
                _sampleService.SaveSample(sampleDto:sampleDto);
            }
            catch (RuleViolationException rve)
            {
                Assert.AreEqual(expected:"ND or NF qualifiers cannot be followed by a value.", actual:rve.ValidationIssues[index:0].ErrorMessage);
            }
        }

        [TestMethod]
        public void SaveSample_ReadyToReport_CalcMassLoading_Numeric_Qualifier_With_Missing_Mass_Unit()
        {
            var sampleDto = GetTestSampleDto();
            var sampleResults = new List<SampleResultDto>();
            var invalidResultDto = new SampleResultDto
                                   {
                                       Qualifier = ">",
                                       EnteredValue = "99",
                                       UnitId = 1,
                                       IsCalcMassLoading = true
                                   };
            sampleResults.Add(item:invalidResultDto);
            sampleDto.SampleResults = sampleResults;
            sampleDto.IsReadyToReport = true;
            try
            {
                _sampleService.SaveSample(sampleDto:sampleDto);
            }
            catch (RuleViolationException rve)
            {
                Assert.AreEqual(expected:"All mass loading calculations must be associated with a valid unit.", actual:rve.ValidationIssues[index:0].ErrorMessage);
            }
        }

        [TestMethod]
        public void SaveSample_ReadyToReport_CalcMassLoading_Numeric_Qualifier_With_Missing_Mass_Value()
        {
            var sampleDto = GetTestSampleDto();
            var sampleResults = new List<SampleResultDto>();
            var invalidResultDto = new SampleResultDto
                                   {
                                       Qualifier = ">",
                                       EnteredValue = "99",
                                       UnitId = 1,
                                       IsCalcMassLoading = true,
                                       MassLoadingUnitId = 1,
                                       MassLoadingValue = null
                                   };
            sampleResults.Add(item:invalidResultDto);
            sampleDto.SampleResults = sampleResults;
            sampleDto.IsReadyToReport = true;
            try
            {
                _sampleService.SaveSample(sampleDto:sampleDto);
            }
            catch (RuleViolationException rve)
            {
                Assert.AreEqual(expected:"You must provide valid a flow value to calculate mass loading results.", actual:rve.ValidationIssues[index:0].ErrorMessage);
            }
        }

        [TestMethod]
        public void GetSampleDetails_With_Results_Valid()
        {
            var sampleId = -1;
            var sampleDtos = _sampleService.GetSamples(status:SampleStatusName.All);
            foreach (var sampleDto in sampleDtos)
            {
                sampleId = sampleDto.SampleId.Value;
                break;
            }

            var firstSampleDto = _sampleService.GetSampleDetails(sampleId:sampleId);
        }

        [TestMethod]
        [ExpectedException(exceptionType:typeof(UnauthorizedAccessException))]
        public void GetSampleDetails_UnauthorizedAccessException()
        {
            var sampleId = 52;
            _httpContext.Setup(s => s.GetClaimValue(It.IsAny<string>())).Returns(value:"4");
            var firstSampleDto = _sampleService.GetSampleDetails(sampleId:sampleId);
        }

        [TestMethod]
        public void Delete_Sample_With_Results_Valid()
        {
            var sampleId = -1;
            var sampleDtos = _sampleService.GetSamples(status:SampleStatusName.All);
            foreach (var sampleDto in sampleDtos)
            {
                sampleId = sampleDto.SampleId.Value;
                break;
            }

            _sampleService.DeleteSample(sampleId:sampleId);
        }

        [TestMethod]
        public void Get_Samples_And_Test_Validity_DRAFT()
        {
            var sampleDtos = _sampleService.GetSamples(status:SampleStatusName.All);
            foreach (var sampleDto in sampleDtos)
            {
                var isValid = _sampleService.IsValidSample(sampleDto:sampleDto, isSuppressExceptions:false);
            }
        }

        [TestMethod]
        public void Get_Samples_And_Test_Validity_READYTOSUBMIT()
        {
            var sampleDtos = _sampleService.GetSamples(status:SampleStatusName.All);
            foreach (var sampleDto in sampleDtos)
            {
                var isValid = _sampleService.IsValidSample(sampleDto:sampleDto, isSuppressExceptions:false);
            }
        }

        [TestMethod]
        public void GetSamples_Only_Reported_Status()
        {
            var sampleDtos = _sampleService.GetSamples(status:SampleStatusName.Reported);
        }

        [TestMethod]
        public void GetSamples_Only_ReadyToReport_Not_Reported_Status()
        {
            var sampleDtos = _sampleService.GetSamples(status:SampleStatusName.ReadyToReport);
        }

        [TestMethod]
        public void Persist_And_Read_Back_SampleDto_And_Compare_Fields()
        {
            //Remove_All_Samples_From_Db();

            //Create test Sample Dto
            var sampleDto = GetTestSampleDto();
            sampleDto.IsMassLoadingResultToUseLessThanSign = true;
            sampleDto.IsReadyToReport = true;
            sampleDto.SampleStatusName = SampleStatusName.ReadyToReport;

            //Persist
            var sampleId = _sampleService.SaveSample(sampleDto:sampleDto);

            //Get
            var fetchedSampleDto = _sampleService.GetSampleDetails(sampleId:sampleId);

            //Compare
            CompareSampleDtos(dto1:sampleDto, dto2:fetchedSampleDto, isAfterUpdate:false);
        }

        [TestMethod]
        public void Update_Existing_Sample()
        {
            var sampleId = 52;

            //Fetch
            var sampleDto = _sampleService.GetSampleDetails(sampleId:sampleId);

            //Change (just increment by 1)
            sampleDto.FlowEnteredValue = (Convert.ToInt32(value:sampleDto.FlowEnteredValue) + 1).ToString();

            //Remove 1 result
            var newList = new List<SampleResultDto>();
            foreach (var item in sampleDto.SampleResults)
            {
                newList.Add(item:item);
            }

            newList.RemoveAt(index:1);

            ////Add a result
            //var resultDto = new SampleResultDto()
            //{
            //    ParameterId = 47,
            //    ParameterName = "2-Nitrophenol",
            //    Qualifier = "<",
            //    UnitId = 7,
            //    UnitName = "mg/L",
            //    Value = "5",
            //    EnteredMethodDetectionLimit = "0.66",
            //    AnalysisMethod = "Analysis Method 66",
            //    AnalysisDateTimeLocal = DateTime.Now,
            //    IsApprovedEPAMethod = true,
            //    IsCalcMassLoading = true,
            //    MassLoadingQualifier = "<",
            //    MassLoadingUnitId = 10,
            //    MassLoadingUnitName = "ppd",
            //    MassLoadingValue = "1.01",
            //};
            //newList.Add(resultDto);
            //resultDto = new SampleResultDto()
            //{
            //    ParameterId = 45,
            //    ParameterName = "2-Methyl-4,6-dinitrophenol",
            //    Qualifier = "<",
            //    UnitId = 7,
            //    UnitName = "mg/L",
            //    Value = "5",
            //    EnteredMethodDetectionLimit = "0.66",
            //    AnalysisMethod = "Analysis Method 66",
            //    AnalysisDateTimeLocal = DateTime.Now,
            //    IsApprovedEPAMethod = true,
            //    IsCalcMassLoading = true
            //};
            //newList.Add(resultDto);

            sampleDto.SampleResults = newList;

            //Persist
            _sampleService.SaveSample(sampleDto:sampleDto);

            //Fetch
            var sampleDto2 = _sampleService.GetSampleDetails(sampleId:sampleId);

            //Compare
            CompareSampleDtos(dto1:sampleDto, dto2:sampleDto2, isAfterUpdate:true);
        }

        [TestMethod]
        public void Missing_Resource_Manager_Entry_Test()
        {
            var keyString = "blahblahblah";
            var missingPhrase = Label.ResourceManager.GetString(name:keyString);

            Assert.IsNull(value:missingPhrase);
        }

        private void CompareSampleDtos(SampleDto dto1, SampleDto dto2, bool isAfterUpdate)
        {
            var type = dto1.GetType();
            var properties = type.GetProperties();

            foreach (var property in properties)
            {
                var fieldName = property.Name;
                var beforeValue = property.GetValue(obj:dto1, index:null);
                var afterValue = property.GetValue(obj:dto2, index:null);

                Debug.WriteLine(message:$"Name: {fieldName}, Before Value: {beforeValue}, After Value: {afterValue}");

                if (fieldName == "SampleId" && !isAfterUpdate) //set within service code
                {
                    //Not passed in for Create New
                    Assert.IsNull(value:beforeValue);
                    int newId;
                    Assert.IsTrue(condition:afterValue != null && int.TryParse(s:afterValue.ToString(), result:out newId) && newId > 0);
                }
                else if (fieldName == "Name" && !isAfterUpdate) //set within service code
                {
                    Assert.IsNull(value:beforeValue);
                    Assert.AreEqual(expected:dto1.CtsEventTypeName, actual:afterValue);
                }
                else if (fieldName == "LastModificationDateTimeLocal") //set within service code
                { }
                else if (fieldName == "LastModifierFullName" && !isAfterUpdate) //set within service code
                { }
                else if (fieldName == "ByOrganizationTypeName" && !isAfterUpdate) //set within service code 
                { }
                else if (fieldName == "FlowUnitValidValues")
                {
                    var fetchedFlowUnitsEnumerator = dto2.FlowUnitValidValues.GetEnumerator();
                    foreach (var unitDto in dto1.FlowUnitValidValues)
                    {
                        fetchedFlowUnitsEnumerator.MoveNext();
                        var fetchedFlowUnit = fetchedFlowUnitsEnumerator.Current;

                        //Compare Results -- just name and id
                        Assert.AreEqual(expected:unitDto.UnitId, actual:fetchedFlowUnit.UnitId);
                        Assert.AreEqual(expected:unitDto.Name, actual:fetchedFlowUnit.Name);
                    }
                }
                else if (fieldName == "SampleResults")
                {
                    var fetchedResultsEnumerator = dto2.SampleResults.GetEnumerator();
                    foreach (var resultDto in dto1.SampleResults)
                    {
                        fetchedResultsEnumerator.MoveNext();
                        var fetchedSampleResult = fetchedResultsEnumerator.Current;

                        //Compare Results
                        var sampleResultDto = resultDto.GetType();
                        var resultProperties = sampleResultDto.GetProperties();
                        foreach (var resultProperty in resultProperties)
                        {
                            var resultFieldName = resultProperty.Name;
                            var resultBeforeValue = resultProperty.GetValue(obj:resultDto, index:null);
                            var resultAfterValue = resultProperty.GetValue(obj:fetchedSampleResult, index:null);

                            Debug.WriteLine(message:$"Name: {resultFieldName}, Before Value: {resultBeforeValue}, After Value: {resultAfterValue}");

                            if (resultFieldName == "ConcentrationSampleResultId" || resultFieldName == "MassLoadingSampleResultId")
                            {
                                //Some results are updates and some are new
                            }
                            else if (resultFieldName == "LastModifierFullName")
                            {
                                //ignore
                            }
                            else if (resultFieldName.Contains(value:"DateTimeLocal")) { }
                            else
                            {
                                Assert.AreEqual(expected:resultBeforeValue, actual:resultAfterValue);
                            }
                        }
                    }
                }
                else
                {
                    Assert.AreEqual(expected:beforeValue, actual:afterValue);
                }
            }
        }

        [TestMethod]
        public void CanUserExecuteApi_GetSampleDetails_AsAuthority_True_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns(value:"authority");

            var sampleId = 2;
            var isAuthorized = _sampleService.CanUserExecuteApi("GetSampleDetails", sampleId);

            Assert.IsTrue(condition:isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_GetSampleDetails_AsAuthority_False_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns(value:"authority");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns(value:"99");

            var sampleId = 2;
            var isAuthorized = _sampleService.CanUserExecuteApi("GetSampleDetails", sampleId);

            Assert.IsFalse(condition:isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_GetSampleDetails_AsIndustry_True_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns(value:"industry");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns(value:"13");

            var sampleId = 2;
            var isAuthorized = _sampleService.CanUserExecuteApi("GetSampleDetails", sampleId);

            Assert.IsTrue(condition:isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_GetSampleDetails_AsIndustry_False_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns(value:"industry");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns(value:"99");

            var sampleId = 2;
            var isAuthorized = _sampleService.CanUserExecuteApi("GetSampleDetails", sampleId);

            Assert.IsFalse(condition:isAuthorized);
        }

        #region Private helper functions

        private SampleDto GetTestSampleDto()
        {
            var sampleDto = new SampleDto
                            {
                                //Name = "Sample XYZ"; //THIS IS NOT SET FROM UI
                                MonitoringPointId = 135,
                                MonitoringPointName = "Process",
                                CollectionMethodId = 1,
                                CollectionMethodName = "24",
                                CtsEventTypeId = 1,
                                CtsEventTypeName = "SNC-P",
                                CtsEventCategoryName = "Sample Category 1",
                                FlowUnitId = 5,
                                FlowUnitName = "gpd",
                                FlowEnteredValue = "25000",
                                StartDateTimeLocal = DateTime.Now.AddDays(value:-7),
                                EndDateTimeLocal = DateTime.Now,
                                IsReadyToReport = false,
                                MassLoadingCalculationDecimalPlaces = 2,
                                MassLoadingConversionFactorPounds = 8.34,
                                IsMassLoadingResultToUseLessThanSign = true,
                                ResultQualifierValidValues = "<,>,ND,NF"
                            };

            var flowUnitValidValues = new List<UnitDto>
                                      {
                                          new UnitDto {UnitId = 5, Name = "gpd", IsFlowUnit = true},
                                          new UnitDto {UnitId = 8, Name = "mgd", IsFlowUnit = true}
                                      };

            sampleDto.FlowUnitValidValues = flowUnitValidValues;

            var resultDtos = new List<SampleResultDto>();

            var resultDto = new SampleResultDto
                            {
                                ParameterId = 14,
                                ParameterName = "1,2-Dibromo-3-chloropropane",
                                Qualifier = "<",
                                UnitId = 7,
                                UnitName = "mg/L",
                                EnteredValue = "0.25",
                                EnteredMethodDetectionLimit = "0.20",
                                AnalysisMethod = "Analysis Method 2",
                                AnalysisDateTimeLocal = DateTime.Now,
                                IsApprovedEPAMethod = true,
                                IsCalcMassLoading = true,
                                MassLoadingQualifier = "<",
                                MassLoadingUnitId = 10,
                                MassLoadingUnitName = "ppd",
                                MassLoadingValue = "52125.00"
                            };
            resultDtos.Add(item:resultDto);
            resultDto = new SampleResultDto
                        {
                            ParameterId = 263,
                            ParameterName = "Trichloroethane",
                            Qualifier = "<",
                            UnitId = 11,
                            UnitName = "su",
                            EnteredValue = "0.99100",
                            EnteredMethodDetectionLimit = "",
                            AnalysisMethod = "Analysis Method 5",
                            AnalysisDateTimeLocal = DateTime.Now,
                            IsApprovedEPAMethod = false,
                            IsCalcMassLoading = false
                        };
            resultDtos.Add(item:resultDto);

            sampleDto.SampleResults = resultDtos;

            return sampleDto;
        }

        [TestMethod]
        public void Remove_All_Samples_From_Db()
        {
            var sampleDtos = _sampleService.GetSamples(status:SampleStatusName.All);
            foreach (var sampleDto in sampleDtos)
            {
                _sampleService.DeleteSample(sampleId:sampleDto.SampleId.Value);
            }
        }

        #endregion
    }
}