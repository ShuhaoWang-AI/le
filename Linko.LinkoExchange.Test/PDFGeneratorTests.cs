using System;
using System.Collections.Generic;
using System.Threading;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services.CopyOfRecord;
using Linko.LinkoExchange.Services.Dto;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class PDFGeneratorTests
    {
        ReportPackageDto _reportPackageDto = new ReportPackageDto();
        private int _monitoringPointCount = 2;

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
            _reportPackageDto.SubmitterTitleRole = "Enironment Compliance Manager";

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

            var pdfGenertator = new PdfGenerator(_reportPackageDto);
            pdfGenertator.CreateCopyOfRecordPdf();
        }

        [TestMethod]
        public void OrderTest1()
        {
            PrepareSampleDtos_OrderTest1();
            PrepareAttachmentTypes();
            PrepareCertifications();
            PrepareReportElementCategoryNames();

            var pdfGenertator = new PdfGenerator(_reportPackageDto);
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
                int seed = (int)DateTime.Now.Ticks;
                int days = seed * random.Next(10) % 90;
                int months = seed * random.Next(10) % 12;

                sampleResults1.Add(new SampleResultDto
                {
                    ParameterName = "1,4-Dioxane",
                    MethodDetectionLimit = 0.02,
                    EnteredValue = random.NextDouble().ToString().Substring(0, 4),
                    UnitName = "mg/L",
                    AnalysisMethod = "200.7",
                    AnalysisDateTimeLocal = DateTimeOffset.Now.DateTime.AddMonths(months).AddDays(days).AddHours(random.Next(3)).AddMinutes(random.Next(10))
                });

                sampleResults1.Add(new SampleResultDto
                {
                    ParameterName = "Benzidine",
                    MethodDetectionLimit = 0.02,
                    EnteredValue = random.NextDouble().ToString().Substring(0, 4),
                    UnitName = "mg/L",
                    AnalysisMethod = "200.7",
                    AnalysisDateTimeLocal = DateTimeOffset.Now.DateTime.AddMonths(months).AddDays(days).AddHours(random.Next(3)).AddMinutes(random.Next(10))
                });

                sampleResults1.Add(new SampleResultDto
                {
                    ParameterName = "1,4-Dioxane",
                    MethodDetectionLimit = 0.02,
                    EnteredValue = random.NextDouble().ToString().Substring(0, 4),
                    UnitName = "mg/L",
                    AnalysisMethod = "200.7",
                    AnalysisDateTimeLocal = DateTimeOffset.Now.DateTime.AddMonths(months).AddDays(days).AddHours(random.Next(3)).AddMinutes(random.Next(10))
                });


                sampleResults1.Add(new SampleResultDto
                {
                    ParameterName = "1,4-Dioxane",
                    MethodDetectionLimit = 0.02,
                    EnteredValue = random.NextDouble().ToString().Substring(0, 4),
                    UnitName = "mg/L",
                    AnalysisMethod = "200.7",
                    AnalysisDateTimeLocal = DateTimeOffset.Now.DateTime.AddMonths(months).AddDays(days).AddHours(random.Next(3)).AddMinutes(random.Next(10))
                });
                //// for sample 2

                sampleResults2.Add(new SampleResultDto
                {
                    ParameterName = "1,4-Dioxane",
                    MethodDetectionLimit = 0.02,
                    EnteredValue = random.NextDouble().ToString().Substring(0, 4),
                    UnitName = "mg/L",
                    AnalysisMethod = "200.7",
                    AnalysisDateTimeLocal = DateTimeOffset.Now.DateTime.AddMonths(months).AddDays(days).AddHours(random.Next(3)).AddMinutes(random.Next(10))
                });

                sampleResults2.Add(new SampleResultDto
                {
                    ParameterName = "bbb",
                    MethodDetectionLimit = 0.02,
                    EnteredValue = random.NextDouble().ToString().Substring(0, 4),
                    UnitName = "mg/L",
                    AnalysisMethod = "200.7",
                    AnalysisDateTimeLocal = DateTimeOffset.Now.DateTime.AddMonths(months).AddDays(days).AddHours(random.Next(3)).AddMinutes(random.Next(10))
                });

                sampleResults2.Add(new SampleResultDto
                {
                    ParameterName = "1,4-Dioxane",
                    MethodDetectionLimit = 0.02,
                    EnteredValue = random.NextDouble().ToString().Substring(0, 4),
                    UnitName = "mg/L",
                    AnalysisMethod = "200.7",
                    AnalysisDateTimeLocal = DateTimeOffset.Now.DateTime.AddMonths(months).AddDays(days).AddHours(random.Next(3)).AddMinutes(random.Next(10))
                });

                sampleResults2.Add(new SampleResultDto
                {
                    ParameterName = "aa",
                    MethodDetectionLimit = 0.02,
                    EnteredValue = random.NextDouble().ToString().Substring(0, 4),
                    UnitName = "mg/L",
                    AnalysisMethod = "200.7",
                    AnalysisDateTimeLocal = DateTimeOffset.Now.DateTime.AddMonths(months).AddDays(days).AddHours(random.Next(3)).AddMinutes(random.Next(10))
                });
            }

            // sampels 
            for (var i = 0; i < _monitoringPointCount; i++)
            {
                int seed = (int)DateTime.Now.Ticks;
                int days = seed * random.Next(90) % 90;
                int month = (seed * random.Next(12) * random.Next()) % 12;
                int hours = seed * random.Next(60) * random.Next() % 60;
                int hours1 = seed * random.Next(60) * random.Next() % 60;
                int minutes = seed * random.Next(60) * random.Next() % 60;
                int minutes1 = seed * random.Next(60) * random.Next() % 60;

                sampleDtos.Add(new SampleDto
                {
                    StartDateTimeLocal = DateTimeOffset.Now.DateTime.AddMonths(month).AddDays(days).AddHours(hours).AddMinutes(minutes),
                    EndDateTimeLocal = DateTimeOffset.Now.DateTime.AddMonths(month).AddDays(days).AddHours(hours + 2).AddMinutes(minutes1),

                    CollectionMethodName = "a-collection method",
                    LabSampleIdentifier = "123232",
                    FlowEnteredValue = "0.18",
                    FlowUnitName = "mgd",
                    MonitoringPointId = 30 + i,
                    MonitoringPointName = $"TestingMonitoringPoint_{i}",
                    SampleResults = sampleResults1
                });

                sampleDtos.Add(new SampleDto
                {
                    StartDateTimeLocal = DateTimeOffset.Now.DateTime.AddMonths(month).AddDays(days).AddHours(hours).AddMinutes(minutes),
                    EndDateTimeLocal = DateTimeOffset.Now.DateTime.AddMonths(month).AddDays(days).AddHours(hours + 2).AddMinutes(minutes1),

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

            _reportPackageDto.SamplesAndResultsTypes.Add(new ReportPackageElementTypeDto
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
                int seed = (int)DateTime.Now.Ticks;
                int days = seed * random.Next(10) % 90;
                int months = seed * random.Next(10) % 12;

                var dt1 = DateTimeOffset.Now.DateTime.AddMonths(months).AddDays(days).AddHours(random.Next(3)).AddMinutes(random.Next(10));
                dt1 = dt1.AddSeconds(-dt1.Second);
                dt1 = dt1.AddMilliseconds(-dt1.Millisecond);

                sampleResults1.Add(new SampleResultDto
                {
                    ParameterName = GetRandomeParameterName(),
                    MethodDetectionLimit = 0.02,
                    EnteredValue = random.NextDouble().ToString().Substring(0, 4),
                    UnitName = "mg/L",
                    AnalysisMethod = "200.7",
                    AnalysisDateTimeLocal = dt1
                });


                var dt2 = DateTimeOffset.Now.DateTime.AddMonths(months + 2).AddDays(days + 3).AddHours(random.Next(3)).AddMinutes(random.Next(10));
                dt2 = dt2.AddSeconds(-dt1.Second);
                dt2 = dt2.AddMilliseconds(-dt1.Millisecond);

                sampleResults2.Add(new SampleResultDto
                {
                    ParameterName = GetRandomeParameterName(),
                    MethodDetectionLimit = 0.02,
                    EnteredValue = random.NextDouble().ToString().Substring(0, 4),
                    UnitName = "mg/L",
                    AnalysisMethod = "200.7",
                    AnalysisDateTimeLocal = dt2
                });


                sampleResults3.Add(new SampleResultDto
                {
                    ParameterName = GetRandomeParameterName(),
                    MethodDetectionLimit = 0.02,
                    EnteredValue = random.NextDouble().ToString().Substring(0, 4),
                    UnitName = "mg/L",
                    AnalysisMethod = "200.7",
                    AnalysisDateTimeLocal = dt2
                });

                sampleResults3.Add(new SampleResultDto
                {
                    ParameterName = GetRandomeParameterName(),
                    IsCalcMassLoading = true,
                    MethodDetectionLimit = 0.02,
                    MassLoadingUnitName = "ppd",
                    MassLoadingValue = random.NextDouble().ToString().Substring(0, 4),
                    EnteredValue = random.NextDouble().ToString().Substring(0, 4),
                    UnitName = "mg/L",
                    AnalysisMethod = "200.7",
                    AnalysisDateTimeLocal = dt2
                });
            }

            // sampels
            for (var i = 0; i < _monitoringPointCount; i++)
            {
                int seed = (int)DateTime.Now.Ticks;
                int days = seed * random.Next(90) % 90;
                int month = (seed * random.Next(12) * random.Next()) % 12;
                int hours = seed * random.Next(60) * random.Next() % 60;
                int minutes = seed * random.Next(60) * random.Next() % 60;
                int minutes1 = seed * random.Next(60) * random.Next() % 60;

                var sample1 = new SampleDto
                {
                    StartDateTimeLocal = DateTimeOffset.Now.DateTime.AddMonths(month).AddDays(days).AddHours(hours).AddMinutes(minutes),
                    EndDateTimeLocal = DateTimeOffset.Now.DateTime.AddMonths(month).AddDays(days).AddHours(hours + 2).AddMinutes(minutes1),

                    CollectionMethodName = GetRandomCollectionMethod(),
                    LabSampleIdentifier = "123232",
                    FlowEnteredValue = "0.18",
                    FlowUnitName = "mgd",
                    MonitoringPointId = 30 + i,
                    MonitoringPointName = $"TestingMonitoringPoint_{i}",
                    SampleResults = sampleResults1
                };
                sampleDtos.Add(sample1);

                var sample2 = new SampleDto
                {
                    StartDateTimeLocal = DateTimeOffset.Now.DateTime.AddMonths(month).AddDays(days).AddHours(hours).AddMinutes(minutes),
                    EndDateTimeLocal = DateTimeOffset.Now.DateTime.AddMonths(month).AddDays(days).AddHours(hours + 2).AddMinutes(minutes1),

                    CollectionMethodName = GetRandomCollectionMethod(),
                    LabSampleIdentifier = "123232",
                    FlowEnteredValue = "0.19",
                    FlowUnitName = "mgd",
                    MonitoringPointId = 30 + i,
                    MonitoringPointName = $"TestingMonitoringPoint_{i}",
                    SampleResults = sampleResults3
                };

                sampleDtos.Add(sample2);

                var sample3 = new SampleDto
                {
                    StartDateTimeLocal = DateTimeOffset.Now.DateTime.AddMonths(month + 3).AddDays(days).AddHours(hours).AddMinutes(minutes),
                    EndDateTimeLocal = DateTimeOffset.Now.DateTime.AddMonths(month + 3).AddDays(days).AddHours(hours + 2).AddMinutes(minutes1),

                    CollectionMethodName = GetRandomCollectionMethod(),
                    LabSampleIdentifier = "123232",
                    FlowEnteredValue = "0.31",
                    FlowUnitName = "mgd",
                    MonitoringPointId = 30 + i,
                    MonitoringPointName = $"TestingMonitoringPoint_{i}",
                    SampleResults = sampleResults2
                };

                sampleDtos.Add(sample3);
            }

            _reportPackageDto.SamplesAndResultsTypes = new List<ReportPackageElementTypeDto>();

            _reportPackageDto.SamplesAndResultsTypes.Add(new ReportPackageElementTypeDto
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
                fileStores.Add(new FileStoreDto
                {
                    FileType = "image/jpg",
                    ReportElementTypeName = $"Lab Analysis Report",
                    Name = $"1st Quarter PCR_{i}",
                    OriginalFileName = $"1st Quarter PCR_{i}"
                });
            }

            _reportPackageDto.AttachmentTypes.Add(new ReportPackageElementTypeDto
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

            _reportPackageDto.CertificationTypes.Add(ttoCertification);
            _reportPackageDto.CertificationTypes.Add(signatureStatement);
        }

        private void PrepareReportElementCategoryNames()
        {
            _reportPackageDto.ReportPackageElementCategories = new List<ReportElementCategoryName>();

            _reportPackageDto.ReportPackageElementCategories.Add(ReportElementCategoryName.Attachments);
            _reportPackageDto.ReportPackageElementCategories.Add(ReportElementCategoryName.Certifications);
            _reportPackageDto.ReportPackageElementCategories.Add(ReportElementCategoryName.SamplesAndResults);
        }


        private string GetRandomCollectionMethod()
        {
            var collectionMethods = new[]
                                    {
                                        "24", "24 hour flow", "8HR", "AVG", "BLANK", "Calcd", "COMP", "DUP", "Each Batch", "Field", "FLOW", "G/C", "GRAB", "IM", "Meter reading", "Trip blank"
                                    };

            int seed = (int)DateTime.Now.Ticks;
            Thread.Sleep(1000);
            var random = new Random(seed);
            var index = random.Next(0, collectionMethods.Length - 1);
            return collectionMethods[index];
        }

        private string GetRandomeParameterName()
        {
            var parameters = GetParameterNames();
            Thread.Sleep(1000);
            int seed = (int)DateTime.Now.Ticks;
            var random = new Random(seed);
            var index = random.Next(0, parameters.Length - 1);
            Console.WriteLine(index);
            return parameters[index];
        }
        private string[] GetParameterNames()
        {
            var parameterNames = new string[]
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
