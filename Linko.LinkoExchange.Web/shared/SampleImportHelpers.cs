using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Web.Routing;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Services.ImportSampleFromFile;
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

		public static string GetDefaultDropdownOptionKey(DataSourceTranslationType translationType)
		{
			return $"default{ImportSampleFromFileService.TranslationTypeSelectListTypeDict[key: translationType]}";
		}

		public static string GetDropdownOptionsKey(DataSourceTranslationType translationType)
		{
			return $"available{ImportSampleFromFileService.TranslationTypeSelectListTypeDict[key: translationType]}s";
		}

        public static ImportSummaryViewModel CreateImportSummary(List<SampleViewModel> samples)
		{
			var importSummary = new ImportSummaryViewModel();
			foreach (var sample in samples)
			{
				var sampleComplianceSummaries = samples.Select(i => i.SampleComplianceSummary).ToList(); 

				importSummary.SampleComplianceSummary.BadConcentrationComplianceCount = sampleComplianceSummaries.Sum(a => a.BadConcentrationComplianceCount);
				importSummary.SampleComplianceSummary.BadMassLoadingComplianceCount = sampleComplianceSummaries.Sum(a => a.BadMassLoadingComplianceCount);

				importSummary.SampleComplianceSummary.GoodConcentrationComplianceCount = sampleComplianceSummaries.Sum(a => a.GoodConcentrationComplianceCount);
				importSummary.SampleComplianceSummary.GoodMassLoadingComplianceCount = sampleComplianceSummaries.Sum(a => a.GoodMassLoadingComplianceCount);
				
				importSummary.SampleComplianceSummary.UnknownConcentrationComplianceCount = sampleComplianceSummaries.Sum(a => a.UnknownConcentrationComplianceCount);
				importSummary.SampleComplianceSummary.UnknownMassLoadingComplianceCount = sampleComplianceSummaries.Sum(a => a.UnknownMassLoadingComplianceCount);

				if (sample.ImportStatus == ImportStatus.Update)
				{
					importSummary.UpdateDraftSampleCont++;
				}
				else
				{
					importSummary.NewDraftSampleCount++;
				}

				foreach (var sampleResult in sample.SampleResults)
				{
					if (sampleResult.ImportStatus == ImportStatus.Update)
					{
						importSummary.UpdateSampleResultCount++;
						if (!string.IsNullOrWhiteSpace(sampleResult.MassLoadingValue))
						{
							importSummary.UpdateSampleResultCount++;
						}
					}
					else
					{
						importSummary.NewSampleResultCount++;
						if (!string.IsNullOrWhiteSpace(value:sampleResult.MassLoadingValue))
						{
							importSummary.NewSampleResultCount++;
						}
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
				       SelectedDefaultSampleTypeId = sampleImportQueryParameters.SelectedDefaultSampleTypeId,
				       ImportSummary = sampleImportQueryParameters.ImportSummary
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
				       SelectedDefaultSampleTypeId = model.SelectedDefaultSampleTypeId,
				       ImportSummary = model.ImportSummary
            };
		}

		public static RouteValueDictionary GetBackRouteValues(SampleImportViewModel model)
		{
			return new RouteValueDictionary
			       {
				       {QueryParameters.ImportJobId, GetBackImportJobId(model:model)}
			       };
		}

		private static string GetBackImportJobId(SampleImportViewModel model)
		{
			var sampleImportQueryParameters = ToSampleImportQueryParameters(model:model);
			return GetBackImportJobId(sampleImportQueryParameters:sampleImportQueryParameters, currentStep:model.CurrentSampleImportStep);
		}

        private static string GetBackImportJobId(SampleImportQueryParameters sampleImportQueryParameters, SampleImportViewModel.SampleImportStep currentStep)
        {
            switch (currentStep)
            {
                case SampleImportViewModel.SampleImportStep.SelectDataSource:
                case SampleImportViewModel.SampleImportStep.SelectFile:
                case SampleImportViewModel.SampleImportStep.FileValidation:
                    sampleImportQueryParameters.CurrentSampleImportStep = SampleImportViewModel.SampleImportStep.SelectDataSource;
                    break;
                case SampleImportViewModel.SampleImportStep.SelectDataDefault:
                    sampleImportQueryParameters.CurrentSampleImportStep = SampleImportViewModel.SampleImportStep.SelectFile;
                    break;
                case SampleImportViewModel.SampleImportStep.DataTranslations:
	                sampleImportQueryParameters.CurrentSampleImportStep = SampleImportViewModel.SampleImportStep.SelectDataDefault;
                    break;
                case SampleImportViewModel.SampleImportStep.DataValidation:
                case SampleImportViewModel.SampleImportStep.ShowPreImportOutput:
                    sampleImportQueryParameters.CurrentSampleImportStep = SampleImportViewModel.SampleImportStep.DataTranslations;
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
			public ImportSummaryViewModel ImportSummary { get; set; }

			#endregion
		}
	}
}