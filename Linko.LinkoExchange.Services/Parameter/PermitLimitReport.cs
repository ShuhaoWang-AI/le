using System;
using System.Collections.Generic;
using System.Linq;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.TimeZone;

namespace Linko.LinkoExchange.Services.Parameter
{
	internal class PermitLimitReport
	{
		#region fields

		private readonly OrganizationRegulatoryProgramDto _authorityRegulatoryProgramDto;
		private readonly OrganizationRegulatoryProgramDto _organizationRegulatoryProgram; 

		#endregion

		#region constructors and destructor

		public PermitLimitReport(List<ParameterLimitsByMonitoringPoint> parammeterByMonitoringPoints,
		                         OrganizationRegulatoryProgramDto orp,
		                         OrganizationRegulatoryProgramDto authorityOrp)
		{
			_organizationRegulatoryProgram = orp;
			_authorityRegulatoryProgramDto = authorityOrp; 
			MonitoringPoints = parammeterByMonitoringPoints.Select(i => new LimitReportMonitoringPoint
			                                                            {
				                                                            MonitoringPointName = i.MonitoringPoint.Name,
				                                                            LimitReportParameterLimits = GetDischargeLimitsFromParameter(monitoringPointParameterLimits:i.ParameterLimits)
			                                                            }).ToList();
		}

		#endregion

		#region public properties

		public string CompanyName => _organizationRegulatoryProgram.OrganizationDto.OrganizationName;
		public string ReportName => "Permit Limits";

		public string IndustryName => _organizationRegulatoryProgram.OrganizationDto.OrganizationName;
		public string IndustryNumber => _organizationRegulatoryProgram.ReferenceNumber;
		public string AddressLine1 => _organizationRegulatoryProgram.OrganizationDto.AddressLine1;
		public string AddressLine2 => _organizationRegulatoryProgram.OrganizationDto.AddressLine2;
		public string City => _organizationRegulatoryProgram.OrganizationDto.CityName;
		public string State => _organizationRegulatoryProgram.OrganizationDto.State;
		public string Zip => _organizationRegulatoryProgram.OrganizationDto.ZipCode;

		public string AuthorityName => _authorityRegulatoryProgramDto.OrganizationDto.OrganizationName;
		public List<LimitReportMonitoringPoint> MonitoringPoints { get; set; }

		#endregion

		private List<LimitReportParameterLimit> GetDischargeLimitsFromParameter(ICollection<MonitoringPointParameterLimit> monitoringPointParameterLimits)
		{
			var parameterLimits = new List<LimitReportParameterLimit>();

			// Group by monitoring point, parameter, effective date, retire date 
			var limitGroups = monitoringPointParameterLimits.GroupBy(i => new
			                                                              {
				                                                              i.MonitoringPointParameter.Parameter.Name,
				                                                              i.MonitoringPointParameter.EffectiveDateTime,
				                                                              i.MonitoringPointParameter.RetirementDateTime
			                                                              }, (key, group) => new
			                                                                                 {
				                                                                                 ParamterName = key.Name,
				                                                                                 key.EffectiveDateTime,
				                                                                                 key.RetirementDateTime,
				                                                                                 limits = group.ToList()
			                                                                                 })
			                                                .OrderBy(j => j.ParamterName)
			                                                .ThenBy(i => i.EffectiveDateTime);

			foreach (var limitGroup in limitGroups)
			{

				var limitReportParameterLimit = new LimitReportParameterLimit
				                                {
					                                ParameterName = limitGroup.ParamterName,
					                                EffectiveDate = limitGroup.EffectiveDateTime,
					                                ExpirationDate = limitGroup.RetirementDateTime
				                                };

				parameterLimits.Add(item:limitReportParameterLimit);

				// Combine concentration and mass loading into single record
				var concentrationDailyLimit = GetConcentrationDailyLimit(limits:limitGroup.limits);
				var massLoadingDailyLimit = GetMassLoadingDailyLimit(limits:limitGroup.limits);

				var concentrationAverageLimit = GetConcentrationAverageLimit(limits:limitGroup.limits);
				var massLoadingAverageLimit = GetMassLoadingAverageLimit(limits:limitGroup.limits);

				limitReportParameterLimit.AverageType = GetAverageType(limits:limitGroup.limits);

				limitReportParameterLimit.ConcentrationDailyLimit = concentrationDailyLimit != null ? (concentrationDailyLimit.MinimumValue.HasValue
					                                                    ? $"{concentrationDailyLimit.MinimumValue.Value}-{concentrationDailyLimit.MaximumValue}"
					                                                    : $"{concentrationDailyLimit.MaximumValue}"): "--";

				limitReportParameterLimit.MassDailyLimit = massLoadingDailyLimit != null
					                                           ? $"{massLoadingDailyLimit.MaximumValue}"
					                                           : "--";

				limitReportParameterLimit.ConcentrationAverageLimit = concentrationAverageLimit != null
					                                                      ? $"{concentrationAverageLimit.MaximumValue}"
					                                                      : "--";
				limitReportParameterLimit.MassAverageLimit = massLoadingAverageLimit != null
					                                             ? $"{massLoadingAverageLimit.MaximumValue}"
					                                             : "--";

				limitReportParameterLimit.ConcentrationUnits = concentrationDailyLimit == null ? string.Empty : concentrationDailyLimit.BaseUnit.Name;
				 
				limitReportParameterLimit.MassUnits = massLoadingDailyLimit == null
					                                      ? (massLoadingAverageLimit == null ? string.Empty : massLoadingAverageLimit.BaseUnit.Name)
					                                      : massLoadingDailyLimit.BaseUnit.Name;
			}

			return parameterLimits;
		}

		private MonitoringPointParameterLimit GetConcentrationDailyLimit(ICollection<MonitoringPointParameterLimit> limits)
		{
			return limits.SingleOrDefault(i => i.LimitBasis.Name.Equals(value:LimitBasisName.Concentration.ToString(), comparisonType:StringComparison.OrdinalIgnoreCase)
			                          && i.LimitType.Name.Equals(value:LimitTypeName.Daily.ToString(), comparisonType:StringComparison.OrdinalIgnoreCase));
		}

		private MonitoringPointParameterLimit GetMassLoadingDailyLimit(ICollection<MonitoringPointParameterLimit> limits)
		{
			return limits.SingleOrDefault(i => i.LimitBasis.Name.Equals(value:LimitBasisName.MassLoading.ToString(), comparisonType:StringComparison.OrdinalIgnoreCase)
			                                   && i.LimitType.Name.Equals(value:LimitTypeName.Daily.ToString(), comparisonType:StringComparison.OrdinalIgnoreCase));
		}

		private MonitoringPointParameterLimit GetConcentrationAverageLimit(ICollection<MonitoringPointParameterLimit> limits)
		{
			return limits.SingleOrDefault(i => i.LimitBasis.Name.Equals(value:LimitBasisName.Concentration.ToString(), comparisonType:StringComparison.OrdinalIgnoreCase) &&
			                                   (i.LimitType.Name.Equals(value:LimitTypeName.FourDay.ToString(), comparisonType:StringComparison.OrdinalIgnoreCase) ||
			                                    i.LimitType.Name.Equals(value:LimitTypeName.Monthly.ToString(), comparisonType:StringComparison.OrdinalIgnoreCase)));
		}

		private MonitoringPointParameterLimit GetMassLoadingAverageLimit(ICollection<MonitoringPointParameterLimit> limits)
		{
			return limits.SingleOrDefault(i => i.LimitBasis.Name.Equals(value:LimitBasisName.MassLoading.ToString(), comparisonType:StringComparison.OrdinalIgnoreCase) &&
			                                   (i.LimitType.Name.Equals(value:LimitTypeName.FourDay.ToString(), comparisonType:StringComparison.OrdinalIgnoreCase) ||
			                                    i.LimitType.Name.Equals(value:LimitTypeName.Monthly.ToString(), comparisonType:StringComparison.OrdinalIgnoreCase)));
		}

		private string GetAverageType(ICollection<MonitoringPointParameterLimit> limits)
		{
			var limit = limits.FirstOrDefault(i => !i.LimitType.Name.Equals(value:LimitTypeName.Daily.ToString(), comparisonType:StringComparison.OrdinalIgnoreCase));
			if (limit == null)
			{
				return string.Empty;
			}

			return limit.LimitType.Name.Equals(value:LimitTypeName.FourDay.ToString(), comparisonType:StringComparison.OrdinalIgnoreCase) ? "F" : "M";
		}
	}

	internal class LimitReportMonitoringPoint
	{
		#region public properties

		public string MonitoringPointName { get; set; }
		public List<LimitReportParameterLimit> LimitReportParameterLimits { get; set; }

		#endregion
	}

	internal class LimitReportParameterLimit
	{
		#region public properties

		public int Id { get; set; }
		public string ParameterName { get; set; }
		public DateTime EffectiveDate { get; set; }
		public DateTime ExpirationDate { get; set; }
		public string ConcentrationDailyLimit { get; set; }
		public string ConcentrationAverageLimit { get; set; }
		public string ConcentrationUnits { get; set; }
		public string AverageType { get; set; }
		public string MassDailyLimit { get; set; }
		public string MassAverageLimit { get; set; }
		public string MassUnits { get; set; }

		#endregion
	}

	internal class ParameterLimitsByMonitoringPoint
	{
		#region public properties

		public Core.Domain.MonitoringPoint MonitoringPoint { get; set; }
		public List<MonitoringPointParameterLimit> ParameterLimits { get; set; }

		#endregion
	}
}