using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Web.Routing;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services.ImportSampleFromFile;
using Linko.LinkoExchange.Web.ViewModels.Industry;
using Linko.LinkoExchange.Web.ViewModels.Shared;
using Microsoft.Ajax.Utilities;
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
                    switch (sampleResult.ImportStatus) {
						case ImportStatus.Update:
							importSummary.UpdateSampleResultCount++;
							if (!string.IsNullOrWhiteSpace(value:sampleResult.MassLoadingValue))
							{
								importSummary.UpdateSampleResultCount++;
							}
							AggregateComplianceSummary(complianceSummary:importSummary.SampleComplianceSummary, sampleResult:sampleResult);
                            break;
						case ImportStatus.New:
							importSummary.NewSampleResultCount++;
							if (!string.IsNullOrWhiteSpace(value:sampleResult.MassLoadingValue))
							{
								importSummary.NewSampleResultCount++;
							}
							AggregateComplianceSummary(complianceSummary:importSummary.SampleComplianceSummary, sampleResult: sampleResult);
                            break;
						case ImportStatus.ExistingUnchanged: break;
						default: throw new ArgumentOutOfRangeException();
					}
				}
			}

			return importSummary;
		}

		private static void AggregateComplianceSummary(SampleComplianceSummaryViewModel complianceSummary, 
													   SampleResultViewModel sampleResult)
		{
			ResultComplianceType concentrationResultCompliance;
			if (!sampleResult.Value.IsNullOrWhiteSpace() && 
			    Enum.TryParse(value:sampleResult.ConcentrationResultCompliance, result:out concentrationResultCompliance))
			{
				switch (concentrationResultCompliance)
				{
					case ResultComplianceType.Good:
						complianceSummary.GoodConcentrationComplianceCount++;
						break;
					case ResultComplianceType.Bad:
						complianceSummary.BadConcentrationComplianceCount++;
						break;
					case ResultComplianceType.Unknown:
						complianceSummary.UnknownConcentrationComplianceCount++;
						break;
					default: throw new ArgumentOutOfRangeException();
				}
			}

			ResultComplianceType massResultCompliance;
			if (sampleResult.MassLoadingValue.IsNullOrWhiteSpace() || 
			    !Enum.TryParse(value:sampleResult.MassResultCompliance, result:out massResultCompliance))
			{
				return;
			}

			switch (massResultCompliance)
			{
				case ResultComplianceType.Good:
					complianceSummary.GoodMassLoadingComplianceCount++;
					break;
				case ResultComplianceType.Bad:
					complianceSummary.BadMassLoadingComplianceCount++;
					break;
				case ResultComplianceType.Unknown:
					complianceSummary.UnknownMassLoadingComplianceCount++;
					break;
				default: throw new ArgumentOutOfRangeException();
			}
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
				       SelectedDefaultMonitoringPointName = sampleImportQueryParameters.SelectedDefaultMonitoringPointName,
				       SelectedDefaultCollectionMethodId = sampleImportQueryParameters.SelectedDefaultCollectionMethodId,
				       SelectedDefaultCollectionMethodName = sampleImportQueryParameters.SelectedDefaultCollectionMethodName,
				       SelectedDefaultSampleTypeId = sampleImportQueryParameters.SelectedDefaultSampleTypeId,
				       SelectedDefaultSampleTypeName = sampleImportQueryParameters.SelectedDefaultSampleTypeName,
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
				       SelectedDefaultMonitoringPointName = model.SelectedDefaultMonitoringPointName,
				       SelectedDefaultCollectionMethodId = model.SelectedDefaultCollectionMethodId,
				       SelectedDefaultCollectionMethodName = model.SelectedDefaultCollectionMethodName,
				       SelectedDefaultSampleTypeId = model.SelectedDefaultSampleTypeId,
				       SelectedDefaultSampleTypeName = model.SelectedDefaultSampleTypeName,
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
			public string SelectedDefaultMonitoringPointName { get; set; }
            public int SelectedDefaultCollectionMethodId { get; set; }
			public string SelectedDefaultCollectionMethodName { get; set; }
            public int SelectedDefaultSampleTypeId { get; set; }
			public string SelectedDefaultSampleTypeName { get; set; }
            public ImportSummaryViewModel ImportSummary { get; set; }

			#endregion
		}
	}
}