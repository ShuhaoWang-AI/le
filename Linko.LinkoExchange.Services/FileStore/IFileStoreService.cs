using System.Collections.Generic;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services
{
    public interface IFileStoreService
    {
        List<string> GetValidAttachmentFileExtensions();
        bool IsValidFileExtension(string ext);

        /// <summary>
        /// Get current user's (identified by OrgRegProgremId) attachment files list
        /// The result doesn't include file data;
        /// </summary>
        /// <returns></returns>
        List<FileStoreDto> GetUserAttachmentFiles();

        /// <summary>
        /// The result includes file byte data
        /// </summary>
        /// <param name="attachenmentFileId">The file Id of the </param>
        /// <returns>One FileStoreDto object</returns>
        FileStoreDto GetFileStoreById(int attachenmentFileId);

        void SaveFileStores(FileStoreDto fileStoreDto);
    }
}
