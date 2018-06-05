﻿using System.Collections.Generic;
using Linko.LinkoExchange.Core.Enum;

namespace Linko.LinkoExchange.Services.Dto
{
    public class SampleImportDto
    {
        #region public properties

        public ImportTempFileDto TempFile { get; set; }
        public DataSourceDto DataSource { get; set; }
        public FileVersionDto FileVersion { get; set; }
        public List<ImportRowObject> Rows { get; set; }
		 
		public List<RequiredDataDefaultsDto> RequiredDefaultValues { get; set; }
		public List<MissingTranslationDto> MissingTranslations { get; set; }

        public List<SampleDto> SampleDtos { get; set; }
        #endregion

	    public SampleImportDto()
	    {
			RequiredDefaultValues = new List<RequiredDataDefaultsDto>();
			MissingTranslations = new List<MissingTranslationDto>();
	    }

        public string ImportJobId => $"Import-{this.TempFile.ImportTempFileId}-{this.DataSource.DataSourceId}";
    }
	
	public class ImportRowObject
	{
		public int RowNumber { get; set; }
		public List<ImportCellObject> Cells { get; set; }
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