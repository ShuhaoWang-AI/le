using System.Collections.Generic;

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
        /// If success is equal to false then "Errors" will have value, otherwise empty string
        /// </summary>
        public IEnumerable<ErrorWithRowNumberDto> Errors { get; set; }

        #endregion
    }
}