﻿namespace Linko.LinkoExchange.Services.Dto
{
    public class AuthorityDetailsDto
    {
        #region public properties

        public string Name { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string CityName { get; set; }
        public int StateId { get; set; }
        public int CountryId { get; set; }
        public string ZipCode { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string Website { get; set; }
        public string Email { get; set; }
        public string NPDESPermitNumber { get; set; }
        public string Signer { get; set; }
        public string UniqueExchangeAuthorityId { get; set; }
        public int MaxUserLicenses { get; set; }
        public int UserLicensesInUse { get; set; }
        public decimal MassPoundsConversionMultiplier { get; set; }
        public int MaxIndustryLicenses { get; set; }
        public int IndustryLicensesInUse { get; set; }
        public int MaxUsersPerIndustry { get; set; }

        #endregion
    }
}