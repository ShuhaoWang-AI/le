using System.Collections.Generic;
using Telerik.Windows.Documents.Spreadsheet.Model;

namespace Linko.LinkoExchange.Services.Dto
{
    public class ImportSampleFromFileValidationResultDto
    {
        #region constructors and destructor

        public ImportSampleFromFileValidationResultDto()
        {
            Success = true;
            Errors = null;
        }

        #endregion

        #region public properties

        public bool Success { get; set; }

        /// <summary>
        /// If success is equal to false then "Errors" will have value, otherwise null 
        /// </summary>
        public IEnumerable<ErrorWithRowNumberDto> Errors { get; set; }

        /// <summary>
        /// If success is equal to true then "importFileWorkbook" will have value, otherwise null
        /// </summary>
        public Workbook ImportFileWorkbook { get; set; }

        #endregion
    }
}