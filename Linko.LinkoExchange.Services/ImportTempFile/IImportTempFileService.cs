using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.ImportTempFile
{
    public interface IImportTempFileService
    {
        
        /// <summary>
        ///     The result includes file byte data
        /// </summary>
        /// <param name="importTempFileId"> The file Id of the </param>
        /// <returns> One ImportTempFileDto object </returns>
        ImportTempFileDto GetImportTempFileById(int importTempFileId);

        /// <summary>
        ///     Adds a new import temp file object to the database.
        /// </summary>
        /// <param name="importTempFileDto"> </param>
        /// <returns> </returns>
        int CreateImportTempFile(ImportTempFileDto importTempFileDto);
    }
}