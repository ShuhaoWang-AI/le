namespace Linko.LinkoExchange.Services.Dto
{
    public class TimeZoneDto
    {
        #region public properties

        public int TimeZoneId { get; set; }
        public string Name { get; set; }
        public string StandardAbbreviation { get; set; }
        public string DaylightAbbreviation { get; set; }

        #endregion
    }
}