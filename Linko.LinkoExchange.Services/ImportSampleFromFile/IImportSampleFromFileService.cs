using Linko.LinkoExchange.Services.Dto;
using Telerik.Windows.Documents.Spreadsheet.Model;

namespace Linko.LinkoExchange.Services.ImportSampleFromFile
{
    public interface IImportSampleFromFileService
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

        /// <summary>
        /// Removes the import file from tImportTempFile table
        /// </summary>
        /// <param name="importTempFileId">File id</param>
        void RemoveImportTempFile(int importTempFileId);

        /// <summary>
        /// Get Telerik.Windows.Documents.Spreadsheet.Model.Workbook from importTempFileDto 
        /// </summary>
        /// <param name="importTempFileDto"></param>
        /// <param name="isAuthorizationRequired">If need to check user have access to the file or not then pass true, otherwise default value is false</param>
        /// <returns></returns>
        Workbook GetWorkbook(ImportTempFileDto importTempFileDto, bool isAuthorizationRequired = false);

        /// <summary>
        /// Checks file is valid to import or not
        /// </summary>
        /// <param name="importTempFileDto"></param>
        /// <returns> 
        ///     ImportSampleFromFileValidationResultDto.Success is equal to "true" if file is valid; otherwise "false"
        ///     If success is equal to "false", then "Errors" will have value, otherwise null 
        ///     If success is equal to "true", then "importFileWorkbook" will have value, otherwise null
        /// </returns>
        ImportSampleFromFileValidationResultDto DoFileValidation(ImportTempFileDto importTempFileDto);
    }
}