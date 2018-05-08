using System.Collections.Generic;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.FileStore
{
    public interface IFileStoreService
    {
        /// <summary>
        ///     Returns a list of strings of valid file extensions.
        /// </summary>
        /// <returns> </returns>
        List<string> GetValidAttachmentFileExtensions();

        /// <summary>
        ///     Compares an extension label against a collection of accepted extensions.
        /// </summary>
        /// <param name="ext"> </param>
        /// <returns> </returns>
        bool IsValidFileExtension(string ext);

        /// <summary>
        ///     Get current user's (identified by OrgRegProgramId) attachment files list
        ///     The result doesn't include file data;
        /// </summary>
        /// <returns> </returns>
        List<FileStoreDto> GetFileStores();

        /// <summary>
        ///     The result includes file byte data
        /// </summary>
        /// <param name="fileStoreId"> The file Id of the </param>
        /// <param name="includingFileData"> Indicates including file data or not </param>
        /// <returns> One FileStoreDto object </returns>
        FileStoreDto GetFileStoreById(int fileStoreId, bool includingFileData = false);

        /// <summary>
        ///     Adds a new file store object to the database after performing validation and size reduction.
        /// </summary>
        /// <param name="fileStoreDto"> </param>
        /// <returns> </returns>
        int CreateFileStore(FileStoreDto fileStoreDto);

        /// <summary>
        ///     Updates an existing file store only if it not already included in any report packages.
        ///     Throws exception an exception if it is already included in a report package.
        /// </summary>
        /// <param name="fileStoreDto"> </param>
        void UpdateFileStore(FileStoreDto fileStoreDto);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileStoreId"> </param>
        void DeleteFileStore(int fileStoreId);

        /// <summary>
        ///     Checks to see if a attachment is associated with at least 1 report package.
        /// </summary>
        /// <param name="fileStore"> </param>
        /// <returns> </returns>
        bool IsFileInReports(int fileStore);

        /// <summary>
        ///     Returns the maximum request size in bytes.
        /// </summary>
        /// <returns> </returns>
        int GetMaxFileSize();
    }
}