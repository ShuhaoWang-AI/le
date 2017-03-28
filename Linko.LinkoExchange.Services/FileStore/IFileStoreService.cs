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
        List<FileStoreDto> GetFileStores();

        /// <summary>
        /// The result includes file byte data
        /// </summary>
        /// <param name="fileStoreId">The file Id of the </param>
        /// <returns>One FileStoreDto object</returns>
        FileStoreDto GetFileStoreById(int fileStoreId);
        int CreateFileStore(FileStoreDto fileStoreDto);

        void UpdateFileStore(FileStoreDto fileStoreDto);
        void DeleteFileStore(int fileStoreId);
    }
}
