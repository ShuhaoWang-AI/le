using System;
using System.Collections.Generic;
using Linko.LinkoExchange.Core.Enum;

namespace Linko.LinkoExchange.Services.Dto
{
    public class SampleImportDto
    {
        #region constructors and destructor

        public SampleImportDto(Guid importId)
        {
            RequiredDefaultValues = new List<RequiredDataDefaultsDto>();
            MissingTranslations = new List<ImportDataTranslationDto>();
            ImportId = importId;
        }

        public SampleImportDto() : this(importId:Guid.NewGuid()) { }

        #endregion

        #region public properties

        public Guid ImportId { get; set; }

        public ImportTempFileDto TempFile { get; set; }
        public DataSourceDto DataSource { get; set; }
        public FileVersionDto FileVersion { get; set; }
        public List<ImportRowObject> Rows { get; set; }

        public List<RequiredDataDefaultsDto> RequiredDefaultValues { get; set; }
        public List<ImportDataTranslationDto> MissingTranslations { get; set; }

        public List<SampleDto> SampleDtos { get; set; }

        public FileStoreDto ImportedFile { get; set; }

        #endregion
    }

    public class ImportRowObject
    {
        #region public properties

        public int RowNumber { get; set; }
        public List<ImportCellObject> Cells { get; set; }

        #endregion
    }

    public class ImportCellObject
    {
        #region public properties

        public SampleImportColumnName SampleImportColumnName { get; set; }
        public string OriginalValueString { get; set; }
        public dynamic OriginalValue { get; set; }
        public dynamic TranslatedValue { get; set; }
        public int TranslatedValueId { get; set; }

        #endregion
    }
}