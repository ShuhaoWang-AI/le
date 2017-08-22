using System;
using System.Collections.Generic;
using System.Threading;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services.CopyOfRecord;
using Linko.LinkoExchange.Services.Dto;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable ArgumentsStyleNamedExpression
// ReSharper disable ArgumentsStyleOther
// ReSharper disable ArgumentsStyleLiteral
// ReSharper disable ArgumentsStyleStringLiteral

namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class PDFGeneratorTests
    {
        #region fields

        private readonly int _monitoringPointCount = 2;
        private readonly ReportPackageDto _reportPackageDto = new ReportPackageDto();

        #endregion

        [TestInitialize]
        public void Init()
        {
            _reportPackageDto.Name = "Test report Name";
            _reportPackageDto.PeriodStartDateTimeLocal = DateTimeOffset.Now.DateTime;
            _reportPackageDto.PeriodEndDateTimeLocal = DateTimeOffset.Now.DateTime;

            _reportPackageDto.OrganizationName = "Valley City Plating, Inc";
            _reportPackageDto.OrganizationReferenceNumber = "40";
            _reportPackageDto.OrganizationAddressLine1 = "3353 N.W. 51";
            _reportPackageDto.OrganizationAddressLine2 = "";
            _reportPackageDto.OrganizationCityName = "Grand Rapids";
            _reportPackageDto.OrganizationJurisdictionName = "MI";
            _reportPackageDto.OrganizationZipCode = "02334";
            _reportPackageDto.SubmissionDateTimeLocal = DateTimeOffset.Now.DateTime;
            _reportPackageDto.SubmitterUserName = "Davlid Pelletier";
            _reportPackageDto.SubmitterTitleRole = "Environment Compliance Manager";

            _reportPackageDto.RecipientOrganizationName = "City of Grand Rapids";

            _reportPackageDto.Comments = "This is comments content ";
        }

        [TestMethod]
        public void GeneralTest()
        {
            PrepareSampleDtos();
            PrepareAttachmentTypes();
            PrepareCertifications();
            PrepareReportElementCategoryNames();

            var pdfGenertator = new PdfGenerator(reportPackage:_reportPackageDto);
            pdfGenertator.CreateCopyOfRecordPdf();
        }

        [TestMethod]
        public void OrderTest1()
        {
            PrepareSampleDtos_OrderTest1();
            PrepareAttachmentTypes();
            PrepareCertifications();
            PrepareReportElementCategoryNames();

            var pdfGenertator = new PdfGenerator(reportPackage:_reportPackageDto);
            pdfGenertator.CreateCopyOfRecordPdf();
        }

        private void PrepareSampleDtos_OrderTest1()
        {
            var sampleResults1 = new List<SampleResultDto>();
            var sampleResults2 = new List<SampleResultDto>();
            var sampleDtos = new List<SampleDto>();

            var random = new Random();

            // samples results
            {
                var seed = (int) DateTime.Now.Ticks;
                var days = seed * random.Next(maxValue:10) % 90;
                var months = seed * random.Next(maxValue:10) % 12;

                sampleResults1.Add(item:new SampleResultDto
                                        {
                                            ParameterName = "1,4-Dioxane",
                                            MethodDetectionLimit = 0.02,
                                            EnteredValue = random.NextDouble().ToString().Substring(startIndex:0, length:4),
                                            UnitName = "mg/L",
                                            AnalysisMethod = "200.7",
                                            AnalysisDateTimeLocal = DateTimeOffset
                                                .Now.DateTime.AddMonths(months:months).AddDays(value:days).AddHours(value:random.Next(maxValue:3))
                                                .AddMinutes(value:random.Next(maxValue:10))
                                        });

                sampleResults1.Add(item:new SampleResultDto
                                        {
                                            ParameterName = "Benzidine",
                                            MethodDetectionLimit = 0.02,
                                            EnteredValue = random.NextDouble().ToString().Substring(startIndex:0, length:4),
                                            UnitName = "mg/L",
                                            AnalysisMethod = "200.7",
                                            AnalysisDateTimeLocal = DateTimeOffset
                                                .Now.DateTime.AddMonths(months:months).AddDays(value:days).AddHours(value:random.Next(maxValue:3))
                                                .AddMinutes(value:random.Next(maxValue:10))
                                        });

                sampleResults1.Add(item:new SampleResultDto
                                        {
                                            ParameterName = "1,4-Dioxane",
                                            MethodDetectionLimit = 0.02,
                                            EnteredValue = random.NextDouble().ToString().Substring(startIndex:0, length:4),
                                            UnitName = "mg/L",
                                            AnalysisMethod = "200.7",
                                            AnalysisDateTimeLocal = DateTimeOffset
                                                .Now.DateTime.AddMonths(months:months).AddDays(value:days).AddHours(value:random.Next(maxValue:3))
                                                .AddMinutes(value:random.Next(maxValue:10))
                                        });

                sampleResults1.Add(item:new SampleResultDto
                                        {
                                            ParameterName = "1,4-Dioxane",
                                            MethodDetectionLimit = 0.02,
                                            EnteredValue = random.NextDouble().ToString().Substring(startIndex:0, length:4),
                                            UnitName = "mg/L",
                                            AnalysisMethod = "200.7",
                                            AnalysisDateTimeLocal = DateTimeOffset
                                                .Now.DateTime.AddMonths(months:months).AddDays(value:days).AddHours(value:random.Next(maxValue:3))
                                                .AddMinutes(value:random.Next(maxValue:10))
                                        });
                //// for sample 2

                sampleResults2.Add(item:new SampleResultDto
                                        {
                                            ParameterName = "1,4-Dioxane",
                                            MethodDetectionLimit = 0.02,
                                            EnteredValue = random.NextDouble().ToString().Substring(startIndex:0, length:4),
                                            UnitName = "mg/L",
                                            AnalysisMethod = "200.7",
                                            AnalysisDateTimeLocal = DateTimeOffset
                                                .Now.DateTime.AddMonths(months:months).AddDays(value:days).AddHours(value:random.Next(maxValue:3))
                                                .AddMinutes(value:random.Next(maxValue:10))
                                        });

                sampleResults2.Add(item:new SampleResultDto
                                        {
                                            ParameterName = "bbb",
                                            MethodDetectionLimit = 0.02,
                                            EnteredValue = random.NextDouble().ToString().Substring(startIndex:0, length:4),
                                            UnitName = "mg/L",
                                            AnalysisMethod = "200.7",
                                            AnalysisDateTimeLocal = DateTimeOffset
                                                .Now.DateTime.AddMonths(months:months).AddDays(value:days).AddHours(value:random.Next(maxValue:3))
                                                .AddMinutes(value:random.Next(maxValue:10))
                                        });

                sampleResults2.Add(item:new SampleResultDto
                                        {
                                            ParameterName = "1,4-Dioxane",
                                            MethodDetectionLimit = 0.02,
                                            EnteredValue = random.NextDouble().ToString().Substring(startIndex:0, length:4),
                                            UnitName = "mg/L",
                                            AnalysisMethod = "200.7",
                                            AnalysisDateTimeLocal = DateTimeOffset
                                                .Now.DateTime.AddMonths(months:months).AddDays(value:days).AddHours(value:random.Next(maxValue:3))
                                                .AddMinutes(value:random.Next(maxValue:10))
                                        });

                sampleResults2.Add(item:new SampleResultDto
                                        {
                                            ParameterName = "aa",
                                            MethodDetectionLimit = 0.02,
                                            EnteredValue = random.NextDouble().ToString().Substring(startIndex:0, length:4),
                                            UnitName = "mg/L",
                                            AnalysisMethod = "200.7",
                                            AnalysisDateTimeLocal = DateTimeOffset
                                                .Now.DateTime.AddMonths(months:months).AddDays(value:days).AddHours(value:random.Next(maxValue:3))
                                                .AddMinutes(value:random.Next(maxValue:10))
                                        });
            }

            // samples 
            for (var i = 0; i < _monitoringPointCount; i++)
            {
                var seed = (int) DateTime.Now.Ticks;
                var days = seed * random.Next(maxValue:90) % 90;
                var month = seed * random.Next(maxValue:12) * random.Next() % 12;
                var hours = seed * random.Next(maxValue:60) * random.Next() % 60;
                var hours1 = seed * random.Next(maxValue:60) * random.Next() % 60;
                var minutes = seed * random.Next(maxValue:60) * random.Next() % 60;
                var minutes1 = seed * random.Next(maxValue:60) * random.Next() % 60;

                sampleDtos.Add(item:new SampleDto
                                    {
                                        StartDateTimeLocal =
                                            DateTimeOffset.Now.DateTime.AddMonths(months:month).AddDays(value:days).AddHours(value:hours).AddMinutes(value:minutes),
                                        EndDateTimeLocal = DateTimeOffset
                                            .Now.DateTime.AddMonths(months:month).AddDays(value:days).AddHours(value:hours + 2).AddMinutes(value:minutes1),

                                        CollectionMethodName = "a-collection method",
                                        LabSampleIdentifier = "123232",
                                        FlowEnteredValue = "0.18",
                                        FlowUnitName = "mgd",
                                        MonitoringPointId = 30 + i,
                                        MonitoringPointName = $"TestingMonitoringPoint_{i}",
                                        SampleResults = sampleResults1
                                    });

                sampleDtos.Add(item:new SampleDto
                                    {
                                        StartDateTimeLocal =
                                            DateTimeOffset.Now.DateTime.AddMonths(months:month).AddDays(value:days).AddHours(value:hours).AddMinutes(value:minutes),
                                        EndDateTimeLocal = DateTimeOffset
                                            .Now.DateTime.AddMonths(months:month).AddDays(value:days).AddHours(value:hours + 2).AddMinutes(value:minutes1),

                                        CollectionMethodName = "b-collection method",
                                        LabSampleIdentifier = "123232",
                                        FlowEnteredValue = "0.19",
                                        FlowUnitName = "mgd",
                                        MonitoringPointId = 30 + i,
                                        MonitoringPointName = $"TestingMonitoringPoint_{i}",
                                        SampleResults = sampleResults2
                                    });
            }

            _reportPackageDto.SamplesAndResultsTypes = new List<ReportPackageElementTypeDto>();

            _reportPackageDto.SamplesAndResultsTypes.Add(item:new ReportPackageElementTypeDto
                                                              {
                                                                  ReportElementTypeName = "Samples and Results",
                                                                  ReportElementTypeContent = string.Empty,
                                                                  ReportElementTypeId = 1,
                                                                  ReportPackageElementCategoryId = 1,
                                                                  Samples = sampleDtos
                                                              });
        }

        private void PrepareSampleDtos()
        {
            var sampleResults1 = new List<SampleResultDto>();
            var sampleResults2 = new List<SampleResultDto>();
            var sampleResults3 = new List<SampleResultDto>();
            var sampleDtos = new List<SampleDto>();

            var random = new Random();
            var sampleResultCount = random.NextDouble() * 20;

            // sample results 
            for (var i = 0; i < sampleResultCount; i++)
            {
                var seed = (int) DateTime.Now.Ticks;
                var days = seed * random.Next(maxValue:10) % 90;
                var months = seed * random.Next(maxValue:10) % 12;

                var dt1 = DateTimeOffset.Now.DateTime.AddMonths(months:months).AddDays(value:days).AddHours(value:random.Next(maxValue:3))
                                        .AddMinutes(value:random.Next(maxValue:10));
                dt1 = dt1.AddSeconds(value:-dt1.Second);
                dt1 = dt1.AddMilliseconds(value:-dt1.Millisecond);

                sampleResults1.Add(item:new SampleResultDto
                                        {
                                            ParameterName = GetRandomeParameterName(),
                                            MethodDetectionLimit = 0.02,
                                            EnteredValue = random.NextDouble().ToString().Substring(startIndex:0, length:4),
                                            UnitName = "mg/L",
                                            AnalysisMethod = "200.7",
                                            AnalysisDateTimeLocal = dt1
                                        });

                var dt2 = DateTimeOffset.Now.DateTime.AddMonths(months:months + 2).AddDays(value:days + 3).AddHours(value:random.Next(maxValue:3))
                                        .AddMinutes(value:random.Next(maxValue:10));
                dt2 = dt2.AddSeconds(value:-dt1.Second);
                dt2 = dt2.AddMilliseconds(value:-dt1.Millisecond);

                sampleResults2.Add(item:new SampleResultDto
                                        {
                                            ParameterName = GetRandomeParameterName(),
                                            MethodDetectionLimit = 0.02,
                                            EnteredValue = random.NextDouble().ToString().Substring(startIndex:0, length:4),
                                            UnitName = "mg/L",
                                            AnalysisMethod = "200.7",
                                            AnalysisDateTimeLocal = dt2
                                        });

                sampleResults3.Add(item:new SampleResultDto
                                        {
                                            ParameterName = GetRandomeParameterName(),
                                            MethodDetectionLimit = 0.02,
                                            EnteredValue = random.NextDouble().ToString().Substring(startIndex:0, length:4),
                                            UnitName = "mg/L",
                                            AnalysisMethod = "200.7",
                                            AnalysisDateTimeLocal = dt2
                                        });

                sampleResults3.Add(item:new SampleResultDto
                                        {
                                            ParameterName = GetRandomeParameterName(),
                                            IsCalcMassLoading = true,
                                            MethodDetectionLimit = 0.02,
                                            MassLoadingUnitName = "ppd",
                                            MassLoadingValue = random.NextDouble().ToString().Substring(startIndex:0, length:4),
                                            EnteredValue = random.NextDouble().ToString().Substring(startIndex:0, length:4),
                                            UnitName = "mg/L",
                                            AnalysisMethod = "200.7",
                                            AnalysisDateTimeLocal = dt2
                                        });
            }

            // samples
            for (var i = 0; i < _monitoringPointCount; i++)
            {
                var seed = (int) DateTime.Now.Ticks;
                var days = seed * random.Next(maxValue:90) % 90;
                var month = seed * random.Next(maxValue:12) * random.Next() % 12;
                var hours = seed * random.Next(maxValue:60) * random.Next() % 60;
                var minutes = seed * random.Next(maxValue:60) * random.Next() % 60;
                var minutes1 = seed * random.Next(maxValue:60) * random.Next() % 60;

                var sample1 = new SampleDto
                              {
                                  StartDateTimeLocal = DateTimeOffset.Now.DateTime.AddMonths(months:month).AddDays(value:days).AddHours(value:hours).AddMinutes(value:minutes),
                                  EndDateTimeLocal = DateTimeOffset.Now.DateTime.AddMonths(months:month).AddDays(value:days).AddHours(value:hours + 2).AddMinutes(value:minutes1),

                                  CollectionMethodName = GetRandomCollectionMethod(),
                                  LabSampleIdentifier = "123232",
                                  FlowEnteredValue = "0.18",
                                  FlowUnitName = "mgd",
                                  MonitoringPointId = 30 + i,
                                  MonitoringPointName = $"TestingMonitoringPoint_{i}",
                                  SampleResults = sampleResults1
                              };
                sampleDtos.Add(item:sample1);

                var sample2 = new SampleDto
                              {
                                  StartDateTimeLocal = DateTimeOffset.Now.DateTime.AddMonths(months:month).AddDays(value:days).AddHours(value:hours).AddMinutes(value:minutes),
                                  EndDateTimeLocal = DateTimeOffset.Now.DateTime.AddMonths(months:month).AddDays(value:days).AddHours(value:hours + 2).AddMinutes(value:minutes1),

                                  CollectionMethodName = GetRandomCollectionMethod(),
                                  LabSampleIdentifier = "123232",
                                  FlowEnteredValue = "0.19",
                                  FlowUnitName = "mgd",
                                  MonitoringPointId = 30 + i,
                                  MonitoringPointName = $"TestingMonitoringPoint_{i}",
                                  SampleResults = sampleResults3
                              };

                sampleDtos.Add(item:sample2);

                var sample3 = new SampleDto
                              {
                                  StartDateTimeLocal = DateTimeOffset.Now.DateTime.AddMonths(months:month + 3).AddDays(value:days).AddHours(value:hours).AddMinutes(value:minutes),
                                  EndDateTimeLocal = DateTimeOffset.Now.DateTime.AddMonths(months:month + 3).AddDays(value:days).AddHours(value:hours + 2)
                                                                   .AddMinutes(value:minutes1),

                                  CollectionMethodName = GetRandomCollectionMethod(),
                                  LabSampleIdentifier = "123232",
                                  FlowEnteredValue = "0.31",
                                  FlowUnitName = "mgd",
                                  MonitoringPointId = 30 + i,
                                  MonitoringPointName = $"TestingMonitoringPoint_{i}",
                                  SampleResults = sampleResults2
                              };

                sampleDtos.Add(item:sample3);
            }

            _reportPackageDto.SamplesAndResultsTypes = new List<ReportPackageElementTypeDto>();

            _reportPackageDto.SamplesAndResultsTypes.Add(item:new ReportPackageElementTypeDto
                                                              {
                                                                  ReportElementTypeName = "Samples and Results",
                                                                  ReportElementTypeContent = string.Empty,
                                                                  ReportElementTypeId = 1,
                                                                  ReportPackageElementCategoryId = 1,
                                                                  Samples = sampleDtos
                                                              });
        }

        private void PrepareAttachmentTypes()
        {
            _reportPackageDto.AttachmentTypes = new List<ReportPackageElementTypeDto>();

            var fileStores = new List<FileStoreDto>();
            for (var i = 0; i < 5; i++)
            {
                fileStores.Add(item:new FileStoreDto
                                    {
                                        FileType = "image/jpg",
                                        ReportElementTypeName = $"Lab Analysis Report",
                                        Name = $"1st Quarter PCR_{i}",
                                        OriginalFileName = $"1st Quarter PCR_{i}"
                                    });
            }

            _reportPackageDto.AttachmentTypes.Add(item:new ReportPackageElementTypeDto
                                                       {
                                                           FileStores = fileStores
                                                       });
        }

        private void PrepareCertifications()
        {
            _reportPackageDto.CertificationTypes = new List<ReportPackageElementTypeDto>();

            var ttoCertification = new ReportPackageElementTypeDto
                                   {
                                       ReportElementTypeName = "TTO Certification",
                                       ReportElementTypeContent = "Based on my inquiry of the person or persons......."
                                   };

            var signatureStatement = new ReportPackageElementTypeDto
                                     {
                                         ReportElementTypeName = "Signature Certification",
                                         ReportElementTypeContent = "I certify ....."
                                     };

            _reportPackageDto.CertificationTypes.Add(item:ttoCertification);
            _reportPackageDto.CertificationTypes.Add(item:signatureStatement);
        }

        private void PrepareReportElementCategoryNames()
        {
            _reportPackageDto.ReportPackageElementCategories = new List<ReportElementCategoryName>();

            _reportPackageDto.ReportPackageElementCategories.Add(item:ReportElementCategoryName.Attachments);
            _reportPackageDto.ReportPackageElementCategories.Add(item:ReportElementCategoryName.Certifications);
            _reportPackageDto.ReportPackageElementCategories.Add(item:ReportElementCategoryName.SamplesAndResults);
        }

        private string GetRandomCollectionMethod()
        {
            var collectionMethods = new[]
                                    {
                                        "24", "24 hour flow", "8HR", "AVG", "BLANK", "Calcd", "COMP", "DUP", "Each Batch", "Field", "FLOW", "G/C", "GRAB", "IM", "Meter reading",
                                        "Trip blank"
                                    };

            var seed = (int) DateTime.Now.Ticks;
            Thread.Sleep(millisecondsTimeout:1000);
            var random = new Random(Seed:seed);
            var index = random.Next(minValue:0, maxValue:collectionMethods.Length - 1);
            return collectionMethods[index];
        }

        private string GetRandomeParameterName()
        {
            var parameters = GetParameterNames();
            Thread.Sleep(millisecondsTimeout:1000);
            var seed = (int) DateTime.Now.Ticks;
            var random = new Random(Seed:seed);
            var index = random.Next(minValue:0, maxValue:parameters.Length - 1);
            Console.WriteLine(value:index);
            return parameters[index];
        }

        private string[] GetParameterNames()
        {
            var parameterNames = new[]
                                 {
                                     "1,1,1,2-Tetrachloroethane",
                                     "1,1,1-Trichloroethane",
                                     "1,1-Dichloroethane",
                                     "1,1-Dichlorpropene",
                                     "1,2,3-Trichlorpropane",

                                     "1,2-Diphenylhydrazine",
                                     "1,3,5-Trimethylbenzene",
                                     "1,3-Dichlorobenzene",
                                     "1,4-Dichlorobenzene",
                                     "1,4-Dioxane",
                                     "1-Chlorobutane",
                                     "2,2-Dichloropropane",
                                     "2,3,7,8-TCDD",

                                     "2,4-Dinitrotoluene",
                                     "2,6-Dinitrotoluene",
                                     "2-Nitropropane",
                                     "3,3-Dichlorobenzidine",
                                     "4,4-DDD (p,p-TDE)",

                                     "4-Isopropyltoluene",
                                     "4-methyl-2-pentanone",
                                     "Acrylic acid",
                                     "Benzene",
                                     "Benzidine",
                                     "Benzo (a) Anthracene",
                                     "Chlorobenzene",
                                     "Chromium, total",
                                     "Chrysene",
                                     "cis-1,2-Dichloroethene",
                                     "cis-1,3-Dichloropropene",

                                     "gamma-BHC (Lindane)",
                                     "gamma-Chlordane",

                                     "Isopropyl ether",

                                     "m/p Cresol",
                                     "Manganese, total",
                                     "Max Daily Flow",

                                     "Molybdenum, total",
                                     "Monthly groundwater flow",
                                     "n-Amyl Acetate",
                                     "n-Butylbenzene",
                                     "N-Nitroso-di-n-propylamine",

                                     "PCB-1221",
                                     "PCB-1232",

                                     "Total Petroleum Hydrocarbons",
                                     "Total Suspended Solids",
                                     "Total Toxic Organics",
                                     "Total VFA",
                                     "Total Volatile Acids",
                                     "Totalizer readings",
                                     "TOX",
                                     "Toxaphene",
                                     "Trans-1,2,-Dichloroethene",

                                     "trans-1,3- Dichloropropene",
                                     "trans-1,3-Dichloropropene",
                                     "trans-1,4-dichloro-2-butene",
                                     "Trichloroethane",
                                     "Trichloroethene",
                                     "Trichloroflouromethane",

                                     "Vinyl acetate",
                                     "Vinyl Chloride",
                                     "Volatile Organic Compounds (EPA 624)",
                                     "Xylene",
                                     "xylenes total",
                                     "Zinc, total"
                                 };

            return parameterNames;
        }
    }
}