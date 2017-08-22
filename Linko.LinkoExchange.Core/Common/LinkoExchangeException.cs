using System;
using System.Collections.Generic;
using Linko.LinkoExchange.Core.Enum;

namespace Linko.LinkoExchange.Core.Common
{
    public class LinkoExchangeException : Exception
    {
        #region public properties

        /// <summary>
        ///     Gets or sets the errors.
        /// </summary>
        /// <value>
        ///     The errors.
        /// </value>
        public IEnumerable<string> Errors { get; set; }

        /// <summary>
        ///     Gets or sets the type of the error.
        /// </summary>
        /// <value>
        ///     The type of the error.
        /// </value>
        public LinkoExchangeError ErrorType { get; set; }

        #endregion
    }
}