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
using System.Diagnostics;
using Linko.LinkoExchange.Web;

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
            sampleDto.MonitoringPointId = 135;
            sampleDto.MonitoringPointName = "Process";
            sampleDto.CollectionMethodId = 1;
            sampleDto.CollectionMethodName = "24";
            sampleDto.CtsEventTypeId = 1;
            sampleDto.CtsEventTypeName = "SNC-P";
            sampleDto.CtsEventCategoryName = "Sample Category 1";
            sampleDto.FlowUnitId = 5;
            sampleDto.FlowUnitName = "gpd";
            sampleDto.FlowValue = "25000";
            sampleDto.StartDateTimeLocal = DateTime.Now.AddDays(-7);
            sampleDto.EndDateTimeLocal = DateTime.Now;
            sampleDto.IsReadyToReport = false;
            sampleDto.MassLoadingCalculationDecimalPlaces = 2;
            sampleDto.MassLoadingConversionFactorPounds = 8.34;
            sampleDto.IsMassLoadingResultToUseLessThanSign = true;
            sampleDto.ResultQualifierValidValues = "<,>,ND,NF";

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
                Value = "0.25",
                EnteredMethodDetectionLimit = "0.20",
                AnalysisMethod = "Analysis Method 2",
                AnalysisDateTimeLocal = DateTime.Now,
                IsApprovedEPAMethod = true,
                IsCalcMassLoading = true,
                MassLoadingQualifier = "<",
                MassLoadingUnitId = 10,
                MassLoadingUnitName = "ppd",
                MassLoadingValue = "52125.00",
            };
            resultDtos.Add(resultDto);
            resultDto = new SampleResultDto()
            {
                ParameterId = 263,
                ParameterName = "Trichloroethane",
                Qualifier = "<",
                UnitId = 11,
                UnitName = "su",
                Value = "0.99100",
                EnteredMethodDetectionLimit = "",
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
        public void SaveSample()
        {
            //Create test Sample Dto
            var sampleDto = GetTestSampleDto();
            sampleDto.IsMassLoadingResultToUseLessThanSign = true;
            sampleDto.IsReadyToReport = true;

            //Persist
            try
            {
               var id =  _sampleService.SaveSample(sampleDto);
               Assert.IsTrue(condition:(id > 0), message:"Sample didn't save");
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
            //Remove_All_Samples_From_Db();

            //Create test Sample Dto
            var sampleDto = GetTestSampleDto();
            sampleDto.IsMassLoadingResultToUseLessThanSign = true;
            sampleDto.IsReadyToReport = true;
            sampleDto.SampleStatusName = SampleStatusName.ReadyToReport;

            //Persist
            int sampleId = _sampleService.SaveSample(sampleDto);

            //Get
            var fetchedSampleDto = _sampleService.GetSampleDetails(sampleId);

            //Compare
            CompareSampleDtos(sampleDto, fetchedSampleDto, false);

        }

        [TestMethod]
        public void Update_Existing_Sample()
        {
            int sampleId = 52;
            //Fetch
            var sampleDto = _sampleService.GetSampleDetails(sampleId);

            //Change (just increment by 1)
            sampleDto.FlowValue = (Convert.ToInt32(sampleDto.FlowValue) + 1).ToString();

            //Remove 1 result
            var newList = new List<SampleResultDto>();
            foreach (var item in sampleDto.SampleResults)
            {
                newList.Add(item);
            }
            newList.RemoveAt(1);

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
            _sampleService.SaveSample(sampleDto);

            //Fetch
            var sampleDto2 = _sampleService.GetSampleDetails(sampleId);

            //Compare
            CompareSampleDtos(sampleDto, sampleDto2, true);
        }


        private void CompareSampleDtos(SampleDto dto1, SampleDto dto2, bool isAfterUpdate)
        {
            Type type = dto1.GetType();
            PropertyInfo[] properties = type.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                var fieldName = property.Name;
                var beforeValue = property.GetValue(dto1, null);
                var afterValue = property.GetValue(dto2, null);

                Debug.WriteLine($"Name: {fieldName}, Before Value: {beforeValue}, After Value: {afterValue}");

                if (fieldName == "SampleId" && !isAfterUpdate) //set within service code
                {
                    //Not passed in for Create New
                    Assert.IsNull(beforeValue);
                    int newId;
                    Assert.IsTrue(afterValue != null && Int32.TryParse(afterValue.ToString(), out newId) && newId > 0);
                }
                else if (fieldName == "Name" && !isAfterUpdate) //set within service code
                {
                    Assert.IsNull(beforeValue);
                    Assert.AreEqual(dto1.CtsEventTypeName, afterValue);
                }
                else if (fieldName == "LastModificationDateTimeLocal") //set within service code
                {

                }
                else if (fieldName == "LastModifierFullName" && !isAfterUpdate) //set within service code
                {

                }
                else if (fieldName == "ByOrganizationTypeName" && !isAfterUpdate) //set within service code 
                {

                }
                else if (fieldName == "FlowUnitValidValues")
                {
                    var fetchedFlowUnitsEnumerator = dto2.FlowUnitValidValues.GetEnumerator();
                    foreach (var unitDto in dto1.FlowUnitValidValues)
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
                    var fetchedResultsEnumerator = dto2.SampleResults.GetEnumerator();
                    foreach (var resultDto in dto1.SampleResults)
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

                            Debug.WriteLine($"Name: {resultFieldName}, Before Value: {resultBeforeValue}, After Value: {resultAfterValue}");

                            if (resultFieldName == "ConcentrationSampleResultId" || resultFieldName == "MassLoadingSampleResultId")
                            {
                                //Some results are updates and some are new
                                string setBreakPointHereToManuallyInspec = "";
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
                else
                {

                    Assert.AreEqual(beforeValue, afterValue);
                }

            }
        }
    }
}
