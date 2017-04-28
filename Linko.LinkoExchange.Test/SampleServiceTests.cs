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
using System.Reflection;
using Linko.LinkoExchange.Services.Unit;
using Linko.LinkoExchange.Services.Config;

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
        Mock<IUnitService> _unitService;

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
            _unitService = new Mock<IUnitService>();

            var actualTimeZoneService = new TimeZoneService(connection, new SettingService(connection, _logger.Object, new MapHelper()), new MapHelper());
            var actualSettings = new SettingService(connection, _logger.Object, new MapHelper());

            _httpContext.Setup(s => s.GetClaimValue(It.IsAny<string>())).Returns("1");
            _orgService.Setup(s => s.GetAuthority(It.IsAny<int>())).Returns(new OrganizationRegulatoryProgramDto() { OrganizationRegulatoryProgramId = 1, OrganizationId = 1000 });

            var actualUnitService = new UnitService(connection,
                new MapHelper(),
                _logger.Object,
                _httpContext.Object,
                actualTimeZoneService,
                _orgService.Object,
                actualSettings);

            _sampleService = new SampleService(connection, 
                _httpContext.Object, 
                _orgService.Object, 
                new MapHelper(), 
                _logger.Object,
                actualTimeZoneService,
                actualSettings,
                actualUnitService);
        }

        #region Private helper functions
        private SampleDto GetTestSampleDto()
        {
            var sampleDto = new SampleDto();
            //sampleDto.Name = "Sample XYZ"; //THIS IS NOT SET FROM UI
            sampleDto.MonitoringPointId = 1;
            sampleDto.MonitoringPointName = "0002-Retired";
            sampleDto.CollectionMethodId = 1;
            sampleDto.CollectionMethodName = "24";
            sampleDto.CtsEventTypeId = 1;
            sampleDto.CtsEventTypeName = "SNC-P";
            sampleDto.CtsEventCategoryName = "Sample Category 1";
            sampleDto.FlowUnitId = 5;
            sampleDto.FlowUnitName = "gpd";
            sampleDto.FlowValue = "808.1";
            sampleDto.StartDateTimeLocal = DateTime.Now;
            sampleDto.EndDateTimeLocal = DateTime.Now;
            sampleDto.IsReadyToReport = false;
            sampleDto.MassLoadingCalculationDecimalPlaces = 4;

            var flowUnitValidValues = new List<UnitDto>();
            flowUnitValidValues.Add(new UnitDto() { UnitId = 5, Name = "gpd", IsFlowUnit = true });
            flowUnitValidValues.Add(new UnitDto() { UnitId = 8, Name = "mgd", IsFlowUnit = true });

            sampleDto.FlowUnitValidValues = flowUnitValidValues;

            var resultDtos = new List<SampleResultDto>();

            var resultDto = new SampleResultDto()
            {
                ParameterId = 14,
                ParameterName = "1,2-Dibromo-3-chloropropane",
                Qualifier = "<",
                UnitId = 7,
                UnitName = "mg/L",
                Value = "20",
                EnteredMethodDetectionLimit = "MDL 2",
                AnalysisMethod = "Analysis Method 2",
                AnalysisDateTimeLocal = DateTime.Now,
                IsApprovedEPAMethod = true,
                IsCalcMassLoading = true,
                MassLoadingQualifier = "<",
                MassLoadingUnitId = 8,
                MassLoadingUnitName = "mgd",
                MassLoadingValue = "1001.2005",
            };
            resultDtos.Add(resultDto);
            resultDto = new SampleResultDto()
            {
                ParameterId = 263,
                ParameterName = "Trichloroethane",
                Qualifier = "<",
                UnitId = 11,
                UnitName = "su",
                Value = "991.00000",
                EnteredMethodDetectionLimit = "MDL 5",
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

        public void Remove_All_Samples_From_Db()
        {
            var sampleDtos = _sampleService.GetSamples(SampleStatusName.All);
            foreach (var sampleDto in sampleDtos)
            {
                _sampleService.DeleteSample(sampleDto.SampleId.Value);
            }
        }

        #endregion

        [TestMethod]
        public void SaveSample_Invalid_Sample_Type()
        {
            var sampleDto = new SampleDto();
            try
            {
                _sampleService.SaveSample(sampleDto);
            }
            catch (RuleViolationException rve)
            {
                
                Assert.AreEqual("Sample Type is required.", rve.ValidationIssues[0].ErrorMessage);

            }
        }

        [TestMethod]
        public void SaveSample_Invalid_Collection_Method()
        {
            var sampleDto = new SampleDto();
            sampleDto.CtsEventTypeId = 1;
            try
            {
                _sampleService.SaveSample(sampleDto);
            }
            catch (RuleViolationException rve)
            {
                
                Assert.AreEqual("Collection Method is required.", rve.ValidationIssues[0].ErrorMessage);

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
                _sampleService.SaveSample(sampleDto);
            }
            catch (RuleViolationException rve)
            {
                
                Assert.AreEqual("Start Date/Time is required.", rve.ValidationIssues[0].ErrorMessage);

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
                _sampleService.SaveSample(sampleDto);
            }
            catch (RuleViolationException rve)
            {
                
                Assert.AreEqual("End Date/Time is required.", rve.ValidationIssues[0].ErrorMessage);

            }
        }

        [TestMethod]
        public void SaveSample_Future_Start_Date()
        {
            var sampleDto = new SampleDto();
            sampleDto.CtsEventTypeId = 1;
            sampleDto.CollectionMethodId = 1;
            sampleDto.StartDateTimeLocal = DateTime.Now.AddDays(1);
            sampleDto.EndDateTimeLocal = DateTime.Now.AddDays(-1);
            try
            {
                _sampleService.SaveSample(sampleDto);
            }
            catch (RuleViolationException rve)
            {
                
                Assert.AreEqual("Sample dates cannot be future dates.", rve.ValidationIssues[0].ErrorMessage);

            }
        }

        [TestMethod]
        public void SaveSample_Future_End_Date()
        {
            var sampleDto = new SampleDto();
            sampleDto.CtsEventTypeId = 1;
            sampleDto.CollectionMethodId = 1;
            sampleDto.StartDateTimeLocal = DateTime.Now.AddDays(-1);
            sampleDto.EndDateTimeLocal = DateTime.Now.AddDays(1);
            try
            {
                _sampleService.SaveSample(sampleDto);
            }
            catch (RuleViolationException rve)
            {
                
                Assert.AreEqual("Sample dates cannot be future dates.", rve.ValidationIssues[0].ErrorMessage);

            }
        }

        [TestMethod]
        public void SaveSample_Details_Concentration_And_Mass_Results_Valid_NotSavingAsReadyToReport()
        {
            var sampleDto = GetTestSampleDto();
            sampleDto.IsReadyToReport = false;
            _sampleService.SaveSample(sampleDto);
        }

        [TestMethod]
        public void SaveSample_ReadyToReport_Mass_Results_Missing_Flow_Unit()
        {
            var sampleDto = GetTestSampleDto();
            sampleDto.IsReadyToReport = true;
            try {
                _sampleService.SaveSample(sampleDto);
            }
            catch (RuleViolationException rve)
            {
                
                Assert.AreEqual("You must provide valid a flow value to calculate mass loading results.", rve.ValidationIssues[0].ErrorMessage);

            }
        }

        [TestMethod]
        public void SaveSample_Flow_Unit_Provided_But_Missing_Flow_Value()
        {
            var sampleDto = GetTestSampleDto();
            sampleDto.FlowUnitId = 1;
            sampleDto.FlowValue = null;
            sampleDto.IsReadyToReport = false;
            try
            {
                _sampleService.SaveSample(sampleDto);
            }
            catch (RuleViolationException rve)
            {
                
                Assert.AreEqual("Flow value and flow unit must be provided together.", rve.ValidationIssues[0].ErrorMessage);

            }
        }

        [TestMethod]
        public void SaveSample_ReadyToReport_NonNumeric_Qualifier_With_Value()
        {
            var sampleDto = GetTestSampleDto();
            var sampleResults = new List<SampleResultDto>();
            var invalidResultDto = new SampleResultDto()
            {
                Qualifier = "ND",
                Value = "99",
                UnitId = 1
            };
            sampleResults.Add(invalidResultDto);
            sampleDto.SampleResults = sampleResults;
            sampleDto.IsReadyToReport = true;
            try
            {
                _sampleService.SaveSample(sampleDto);
            }
            catch (RuleViolationException rve)
            {
                
                Assert.AreEqual("ND or NF qualifiers cannot be followed by a value.", rve.ValidationIssues[0].ErrorMessage);

            }
        }

        [TestMethod]
        public void SaveSample_ReadyToReport_CalcMassLoading_Numeric_Qualifier_With_Missing_Mass_Unit()
        {
            var sampleDto = GetTestSampleDto();
            var sampleResults = new List<SampleResultDto>();
            var invalidResultDto = new SampleResultDto()
            {
                Qualifier = ">",
                Value = "99",
                UnitId = 1,
                IsCalcMassLoading = true
            };
            sampleResults.Add(invalidResultDto);
            sampleDto.SampleResults = sampleResults;
            sampleDto.IsReadyToReport = true;
            try
            {
                _sampleService.SaveSample(sampleDto);
            }
            catch (RuleViolationException rve)
            {

                Assert.AreEqual("All mass loading calculations must be associated with a valid unit.", rve.ValidationIssues[0].ErrorMessage);

            }
        }

        [TestMethod]
        public void SaveSample_ReadyToReport_CalcMassLoading_Numeric_Qualifier_With_Missing_Mass_Value()
        {
            var sampleDto = GetTestSampleDto();
            var sampleResults = new List<SampleResultDto>();
            var invalidResultDto = new SampleResultDto()
            {
                Qualifier = ">",
                Value = "99",
                UnitId = 1,
                IsCalcMassLoading = true,
                MassLoadingUnitId = 1,
                MassLoadingValue = null
            };
            sampleResults.Add(invalidResultDto);
            sampleDto.SampleResults = sampleResults;
            sampleDto.IsReadyToReport = true;
            try
            {
                _sampleService.SaveSample(sampleDto);
            }
            catch (RuleViolationException rve)
            {

                Assert.AreEqual("You must provide valid a flow value to calculate mass loading results.", rve.ValidationIssues[0].ErrorMessage);

            }
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
                var isValid = _sampleService.IsValidSample(sampleDto, false);
            }
        }

        [TestMethod]
        public void Get_Samples_And_Test_Validity_READYTOSUBMIT()
        {
            var sampleDtos = _sampleService.GetSamples(SampleStatusName.All);
            foreach (var sampleDto in sampleDtos)
            {
                var isValid = _sampleService.IsValidSample(sampleDto, false);
            }
        }

        [TestMethod]
        public void GetSamples_Only_Reported_Status()
        {
            var sampleDtos = _sampleService.GetSamples(SampleStatusName.Reported);
            
        }

        [TestMethod]
        public void Persist_And_Read_Back_SampleDto_And_Compare_Fields()
        {
            Remove_All_Samples_From_Db();

            //Create test Sample Dto
            var sampleDto = GetTestSampleDto();
            sampleDto.IsMassLoadingResultToUseLessThanSign = true;
            sampleDto.IsReadyToReport = true;

            //Persist
            int sampleId = _sampleService.SaveSample(sampleDto);

            //Get
            var fetchedSampleDto = _sampleService.GetSampleDetails(sampleId);

            //Compare
            Type type = sampleDto.GetType();
            PropertyInfo[] properties = type.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                var fieldName = property.Name;
                var beforeValue = property.GetValue(sampleDto, null);
                var afterValue = property.GetValue(fetchedSampleDto, null);

                Console.WriteLine($"Name: {fieldName}, Before Value: {beforeValue}, After Value: {afterValue}");

                if (fieldName == "SampleId") //set within service code
                {

                }
                else if (fieldName == "Name") //set within service code
                {
                    Assert.IsNull(beforeValue);
                    Assert.AreEqual(sampleDto.CtsEventTypeName, afterValue);
                }
                else if (fieldName == "LastModificationDateTimeLocal") //set within service code
                {

                }
                else if (fieldName == "LastModifierFullName") //set within service code
                {

                }
                else if (fieldName == "ByOrganizationRegulatoryProgramDto")
                {

                }
                else if (fieldName == "FlowUnitValidValues")
                {
                    var fetchedFlowUnitsEnumerator = fetchedSampleDto.FlowUnitValidValues.GetEnumerator();
                    foreach (var unitDto in sampleDto.FlowUnitValidValues)
                    {
                        fetchedFlowUnitsEnumerator.MoveNext();
                        var fetchedFlowUnit = fetchedFlowUnitsEnumerator.Current;

                        //Compare Results -- just name and id
                        Assert.AreEqual(unitDto.UnitId, fetchedFlowUnit.UnitId);
                        Assert.AreEqual(unitDto.Name, fetchedFlowUnit.Name);
                    }
                }
                else if (fieldName == "SampleResults")
                {
                    var fetchedResultsEnumerator = fetchedSampleDto.SampleResults.GetEnumerator();
                    foreach (var resultDto in sampleDto.SampleResults)
                    {
                        fetchedResultsEnumerator.MoveNext();
                        var fetchedSampleResult = fetchedResultsEnumerator.Current;

                        //Compare Results
                        Type sampleResultDto = resultDto.GetType();
                        PropertyInfo[] resultProperties = sampleResultDto.GetProperties();
                        foreach (PropertyInfo resultProperty in resultProperties)
                        {
                            var resultFieldName = resultProperty.Name;
                            var resultBeforeValue = resultProperty.GetValue(resultDto, null);
                            var resultAfterValue = resultProperty.GetValue(fetchedSampleResult, null);

                            if (resultFieldName == "SampleId")
                            {
                                //ignore
                            }
                            else if (resultFieldName == "LastModifierFullName")
                            {
                                //ignore
                            }
                            else if (resultFieldName.Contains("DateTimeLocal"))
                            {

                            }
                            else
                            {
                                Assert.AreEqual(resultBeforeValue, resultAfterValue);
                            }
                        }

                    }
                   
                }
                else {

                    Assert.AreEqual(beforeValue, afterValue);
                }
                
            }

        }
    }
}
