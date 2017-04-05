
namespace Linko.LinkoExchange.Web.ViewModels.Attachment
{
    public class AttachmentModel
    {
        public int FileStoreId
        {
            get; set;
        }
        public string FileName
        {
            get; set;
        }

        public string MediaType { get; set; }

        public string OriginFileName { get; set; }

        public string AttachmentTypeName
        {
            get; set;
        }

        public string Description
        {
            get; set;
        }
    }
}