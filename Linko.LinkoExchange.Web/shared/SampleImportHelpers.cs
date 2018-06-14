using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Web.Routing;
using Linko.LinkoExchange.Web.ViewModels.Industry;
using Linko.LinkoExchange.Web.ViewModels.Shared;
using Newtonsoft.Json;

namespace Linko.LinkoExchange.Web.Shared
{
    public static class SampleImportHelpers
    {
        #region static fields and constants

        private static readonly JsonSerializerSettings IgnoreNullValues = new JsonSerializerSettings
                                                                          {
                                                                              NullValueHandling = NullValueHandling.Ignore
                                                                          };

        #endregion

        public static ImportSummaryViewModel CreateImportSummary(List<SampleViewModel> samples)
        {
            var importSummary = new ImportSummaryViewModel();

            foreach (var sample in samples)
            {
				//TODO to populate compliance summary 

                if (sample.ImportStatus == ImportStatus.Updated)
                {
                    importSummary.UpdateDraftSampleCont++;
                }
                else
                {
                    importSummary.NewDraftSampleCount++;
                }

                foreach (var sampleResult in sample.SampleResults)
                {
                    if (sampleResult.ImportStatus == ImportStatus.Updated)
                    {
                        importSummary.UpdateSampleResultCount++;
                    }
                    else
                    {
                        importSummary.NewSampleResultCount++;
                    }
                }
            }

            return importSummary;
        }

        public static SampleImportViewModel FromImportJobId(string importJobId)
        {
            if (string.IsNullOrEmpty(value:importJobId))
            {
                return null;
            }

            var sampleImportQueryParameters = Decode<SampleImportQueryParameters>(encodedData:importJobId);
            return new SampleImportViewModel
                   {
                       ImportId = sampleImportQueryParameters.ImportId,
                       CurrentSampleImportStep = sampleImportQueryParameters.CurrentSampleImportStep,
                       ImportTempFileId = sampleImportQueryParameters.ImportTempFileId,
                       SelectedDataSourceId = sampleImportQueryParameters.SelectedDataSourceId,
                       SelectedDefaultMonitoringPointId = sampleImportQueryParameters.SelectedDefaultMonitoringPointId,
                       SelectedDefaultCollectionMethodId = sampleImportQueryParameters.SelectedDefaultCollectionMethodId,
                       SelectedDefaultSampleTypeId = sampleImportQueryParameters.SelectedDefaultSampleTypeId
                   };
        }

        public static string ToImportJobId(SampleImportViewModel model)
        {
            var sampleImportQueryParameters = ToSampleImportQueryParameters(model:model);

            return Encode(objectToEncode:sampleImportQueryParameters);
        }

        public static SampleImportQueryParameters ToSampleImportQueryParameters(SampleImportViewModel model)
        {
            return new SampleImportQueryParameters
                   {
                       ImportId = model.ImportId,
                       CurrentSampleImportStep = model.CurrentSampleImportStep,
                       ImportTempFileId = model.ImportTempFileId,
                       SelectedDataSourceId = model.SelectedDataSourceId,
                       SelectedDefaultMonitoringPointId = model.SelectedDefaultMonitoringPointId,
                       SelectedDefaultCollectionMethodId = model.SelectedDefaultCollectionMethodId,
                       SelectedDefaultSampleTypeId = model.SelectedDefaultSampleTypeId
                   };
        }

        public static RouteValueDictionary GetBackRouteValues(SampleImportViewModel model)
        {
            return new RouteValueDictionary
                   {
                       {QueryParameters.ImportJobId,  GetBackImportJobId(model:model)}
                   };
        }
        private static string GetBackImportJobId(SampleImportViewModel model)
        {
            var sampleImportQueryParameters = ToSampleImportQueryParameters(model:model);
            return GetBackImportJobId(sampleImportQueryParameters:sampleImportQueryParameters, currentStep: model.CurrentSampleImportStep);
        }

        private static string GetBackImportJobId(SampleImportQueryParameters sampleImportQueryParameters, SampleImportViewModel.SampleImportStep currentStep)
        {
            switch (currentStep)
            {
                case SampleImportViewModel.SampleImportStep.SelectDataSource:
                case SampleImportViewModel.SampleImportStep.SelectFile:
                    sampleImportQueryParameters.CurrentSampleImportStep = SampleImportViewModel.SampleImportStep.SelectDataSource;
                    break;
                case SampleImportViewModel.SampleImportStep.FileValidation:
                case SampleImportViewModel.SampleImportStep.SelectDataDefault:
                    sampleImportQueryParameters.CurrentSampleImportStep = SampleImportViewModel.SampleImportStep.SelectFile;
                    break;
                case SampleImportViewModel.SampleImportStep.DataTranslations:
                case SampleImportViewModel.SampleImportStep.DataValidation:
                case SampleImportViewModel.SampleImportStep.ShowPreImportOutput:
                    var anyDefaultValuesAssigned = sampleImportQueryParameters.SelectedDefaultMonitoringPointId > 0
                                                   || sampleImportQueryParameters.SelectedDefaultCollectionMethodId > 0
                                                   || sampleImportQueryParameters.SelectedDefaultSampleTypeId > 0;
                    sampleImportQueryParameters.CurrentSampleImportStep =
                        anyDefaultValuesAssigned ? SampleImportViewModel.SampleImportStep.SelectDataDefault : SampleImportViewModel.SampleImportStep.SelectFile;
                    break;
                case SampleImportViewModel.SampleImportStep.ShowImportOutput:
                default: throw new ArgumentOutOfRangeException();
            }

            return Encode(objectToEncode:sampleImportQueryParameters);
        }

        public static string ToJsonString(dynamic value)
        {
            return JsonConvert.SerializeObject(value:value, settings:IgnoreNullValues);
        }

        private static string Encode<T>(T objectToEncode)
        {
            var jsonStr = ToJsonString(value:objectToEncode);
            using (var inputStream = new MemoryStream(buffer:Encoding.UTF8.GetBytes(s:jsonStr)))
            {
                using (var outputStream = new MemoryStream())
                {
                    using (var gZipStream = new GZipStream(stream:outputStream, mode:CompressionMode.Compress))
                    {
                        CopyTo(src:inputStream, dest:gZipStream);
                    }

                    return Convert.ToBase64String(inArray:outputStream.ToArray());
                }
            }
        }

        private static void CopyTo(Stream src, Stream dest)
        {
            var bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(buffer:bytes, offset:0, count:bytes.Length)) != 0)
            {
                dest.Write(buffer:bytes, offset:0, count:cnt);
            }
        }

        private static T Decode<T>(string encodedData)
        {
            using (var inputStream = new MemoryStream(buffer:Convert.FromBase64String(s:encodedData)))
            {
                using (var outputStream = new MemoryStream())
                {
                    using (var gZipStream = new GZipStream(stream:inputStream, mode:CompressionMode.Decompress))
                    {
                        CopyTo(src:gZipStream, dest:outputStream);
                    }

                    var jsonString = Encoding.UTF8.GetString(bytes:outputStream.ToArray());

                    return JsonConvert.DeserializeObject<T>(value:jsonString, settings:IgnoreNullValues);
                }
            }
        }

        [Serializable]
        public class SampleImportQueryParameters
        {
            #region public properties

            public Guid ImportId { get; set; }
            public SampleImportViewModel.SampleImportStep CurrentSampleImportStep { get; set; }
            public int SelectedDataSourceId { get; set; }
            public int ImportTempFileId { get; set; }
            public int ImportFileId { get; set; }
            public int SelectedDefaultMonitoringPointId { get; set; }
            public int SelectedDefaultCollectionMethodId { get; set; }
            public int SelectedDefaultSampleTypeId { get; set; }

            #endregion
        }
    }
}