using System.Collections.Generic;

namespace Linko.LinkoExchange.Services.Dto
{
    public class ReportPackageElementTypeDto
    {
        public int ReportPackageElementTypeId { get; set; }
        public int ReportPackageElementCategoryId { get; set; }
        public int ReportElementTypeId { get; set; }
        public string ReportElementTypeName { get; set; }
        public string ReportElementTypeContent { get; set; }
        
        public bool ReportElementTypeIsContentProvided { get; set; }
        public int? CtsEventTypeId { get; set; }
        public string CtsEventTypeName { get; set; }
        public string CtsEventCategoryName { get; set; }
        public bool IsRequired { get; set; }
        public int SortOrder { get; set; }

        public List<SampleDto> Samples { get; set; }
        public List<FileStoreDto> FileStores { get; set; }
    }
}
