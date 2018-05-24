using System.Collections.Generic;
using System.Linq;

namespace Linko.LinkoExchange.Services.Dto
{
    public class ImportSampleFromFileValidationResultDto
    {
        #region constructors and destructor

        public ImportSampleFromFileValidationResultDto()
        {
            Errors = new List<ErrorWithRowNumberDto>();
        }

        #endregion

        #region public properties

	    public bool Success => !Errors.Any();

	    /// <summary>
        /// If success is equal to false then "Errors" will have value, otherwise empty string
        /// </summary>
        public List<ErrorWithRowNumberDto> Errors { get; set; }

        #endregion
    }
}