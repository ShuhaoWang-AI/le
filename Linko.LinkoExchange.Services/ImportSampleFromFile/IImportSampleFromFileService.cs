using System.Collections.Generic;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.ImportSampleFromFile
{
    public interface IImportSampleFromFileService
    {
        /// <summary>
        /// The result includes file byte data
        /// </summary>
        /// <param name="importTempFileId"> The file Id of the </param>
        /// <returns> One ImportTempFileDto object </returns>
        ImportTempFileDto GetImportTempFileById(int importTempFileId);

        /// <summary>
        /// Adds a new import temp file object to the database.
        /// </summary>
        /// <param name="importTempFileDto"> </param>
        /// <returns> </returns>
        int CreateImportTempFile(ImportTempFileDto importTempFileDto);

        /// <summary>
        /// Removes the import file from tImportTempFile table
        /// </summary>
        /// <param name="importTempFileId"> File id </param>
        void RemoveImportTempFile(int importTempFileId);

        /// <summary>
        /// Checks file is valid to import or not
        /// </summary>
        /// <param name="sampleImportDto"> </param>
        /// <returns>
        /// ImportSampleFromFileValidationResultDto.Success is equal to "true" if file is valid; otherwise "false"
        /// If success is equal to "false", then "Errors" will have value, otherwise null
        /// </returns>
        ImportSampleFromFileValidationResultDto DoFileValidation(SampleImportDto sampleImportDto);

        /// <summary>
        /// Checks sampleImportDto has any recommended column cell has missing value or not
        /// If missing then return list of RequiredDataDefaultsDto
        /// </summary>
        /// <param name="sampleImportDto"> </param>
        /// <returns> </returns>
        List<RequiredDataDefaultsDto> GetRequiredDataDefaults(SampleImportDto sampleImportDto);

        void PopulateDataDefaults(SampleImportDto sampleImportDto, ListItemDto defaultMonitoringPoint, ListItemDto defaultCollectionMethod, ListItemDto defaultSampleType);

        List<ImportDataTranslationDto> PopulateExistingTranslationDataAndReturnMissingTranslationSet(SampleImportDto sampleImportDto, int dataSourceId);

        ImportSampleFromFileValidationResultDto DoDataValidation(SampleImportDto sampleImportDto);

        FileVersionDto GetFileVersion(string fileVersionName);

        void ImportSampleAndCreateAttachment(SampleImportDto sampleImportDto);

        FileVersionDto GetFileVersionForAuthorityConfiguration(FileVersionTemplateName fileVersionTemplateName);
        int? AddOrUpdateFileVersionFieldForAuthorityConfiguration(int fileVersionId, FileVersionFieldDto dto);
        ExportFileDto DownloadSampleImportTemplate(string fileVersionName);
        ExportFileDto DownloadSampleImportTemplateInstruction(string fileVersionName);

    }
}