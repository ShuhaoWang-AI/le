namespace Linko.LinkoExchange.Services.Dto
{
    public class JurisdictionDto
    {
        #region public properties

        public int JurisdictionId { get; set; }
        public int CountryId { get; set; }
        public int StateId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int ParentId { get; set; }

        #endregion
    }
}