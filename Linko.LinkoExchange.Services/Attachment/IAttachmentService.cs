using System.Collections.Generic;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.Attachment
{
    public interface IAttachmentService
    {
        List<string> GetValidAttachmentFileExtensions();
        bool IsValidFileExtension(string ext);


        /// <summary>
        /// Get current user's (identified by OrgRegProgremId) attachment files list
        /// </summary>
        /// <returns></returns>
        IList<string> GetUserAttachmentFiles();

        void SaveAttachmentFile(AttachmentFileDto fileDto);
    }
}
