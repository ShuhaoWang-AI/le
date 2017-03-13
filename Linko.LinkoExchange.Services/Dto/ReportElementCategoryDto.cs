using Linko.LinkoExchange.Services.Dto;
using System;

namespace Linko.LinkoExchange.Services.Dto
{
    public class ReportElementCategoryDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTimeOffset CreationDateTimeUtc { get; set; }
        public DateTimeOffset LastModificationDateUtc { get; set; }
        public UserDto LastModifierUser { get; set; }
    }
}