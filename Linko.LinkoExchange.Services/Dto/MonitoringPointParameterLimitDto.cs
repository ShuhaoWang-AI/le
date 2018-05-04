namespace Linko.LinkoExchange.Services.Dto
{
	public class MonitoringPointParameterLimitDto
	{
		public int MonitoringPointParameterLimitId { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public double? MinimumValue { get; set; }
		public double MaximumValue { get; set; }
		public int BaseUnitId { get; set; }
		public UnitDto BaseUnitDto { get; set; }
		public int? CollectionMethodId { get; set; }
		public CollectionMethodDto CollectionMethod { get; set; }
		public int LimitTypeId { get; set; }
		public LimitTypeDto LimitType { get; set; }
		public int LimitBasisId { get; set; }
		public LimitBasisDto LimitBasis { get; set; }
		public bool IsAlertOnly { get; set; }
	}
}