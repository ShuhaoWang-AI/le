using Linko.LinkoExchange.Core.Enum;

namespace Linko.LinkoExchange.Services.Dto
{
    public class FileVersionFieldDto
    {
        #region public properties

        public int? FileVersionFieldId { get; set; }
        public int SystemFieldId { get; set; }
        public SystemFieldName SystemFieldName { get; set; }
        public string FileVersionFieldName { get; set; }
        public string Description { get; set; }
        public DataOptionalityName DataOptionalityName { get; set; }
        public bool IsSystemRequired { get; set; }
        public int Size { get; set; }
        public string ExampleData { get; set; }
        public string AdditionalComments { get; set; }

        #endregion
    }
}