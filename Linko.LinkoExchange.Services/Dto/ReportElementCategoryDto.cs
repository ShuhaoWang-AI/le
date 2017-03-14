using Linko.LinkoExchange.Services.Dto;
using System;

namespace Linko.LinkoExchange.Services.Dto
{
    public class ReportElementCategoryDto
    {
        public int ReportElementCategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTimeOffset CreationDateTimeUtc { get; set; }
        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }
        public UserDto LastModifierUser { get; set; }
        public int? LastModifierUserId { get; set; }
    }
}